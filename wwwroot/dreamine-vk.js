// Dreamine.UI.Blazor virtual keyboard JS helper (ES module).
// Loaded dynamically by Blazor via import() so host apps do not need a <script> tag.
// Tracks the most recently focused text-like input (<input type=text|password>, <textarea>)
// so the virtual keyboard can edit it directly.

let _lastActive = null;
let _keyboardSubscribers = new Set();
let _shiftPressed = false;
let _capsLockOn = false;

function isEditableInput(el) {
    if (!el || !(el instanceof HTMLElement)) return false;
    if (el.readOnly || el.disabled) return false;
    const tag = el.tagName;
    if (tag === 'TEXTAREA') return true;
    if (tag === 'INPUT') {
        const type = (el.getAttribute('type') || 'text').toLowerCase();
        return type === 'text' || type === 'password' || type === 'search' || type === 'url' || type === 'email' || type === 'tel';
    }
    return false;
}

function captureIfEditable(el) {
    if (isEditableInput(el)) {
        _lastActive = el;
    }
}

function isVirtualKeyboardTarget(el) {
    return el instanceof HTMLElement && !!el.closest('.dreamine-vk');
}

// The module is imported dynamically by the virtual keyboard component AFTER
// the user has already focused the target input. That means the initial focusin
// event has fired before this listener even exists, so _lastActive stays null
// until the user re-focuses. Fix: seed _lastActive from document.activeElement
// at load time.
captureIfEditable(document.activeElement);

// Track focus from multiple angles so we do not lose the target:
//   - focusin bubbles for programmatic and click-driven focus changes,
//   - pointerdown/mousedown/touchstart fire before focus and catch the
//     user's intent even if focus is stolen or delayed.
document.addEventListener('focusin', (ev) => {
    if (isVirtualKeyboardTarget(ev.target) && _lastActive && _lastActive.isConnected) {
        try { _lastActive.focus({ preventScroll: true }); } catch (_) { }
        return;
    }

    captureIfEditable(ev.target);
}, true);
function preserveInputFocusForVirtualKey(ev) {
    if (!isVirtualKeyboardTarget(ev.target)) {
        captureIfEditable(ev.target);
        return;
    }

    if (_lastActive && _lastActive.isConnected) {
        try {
            ev.preventDefault();
            _lastActive.focus({ preventScroll: true });
        } catch (_) { }
    }
}

document.addEventListener('pointerdown', preserveInputFocusForVirtualKey, true);
document.addEventListener('mousedown', preserveInputFocusForVirtualKey, true);
document.addEventListener('touchstart', preserveInputFocusForVirtualKey, true);

function notifyKeyboardState() {
    for (const subscriber of Array.from(_keyboardSubscribers)) {
        try {
            subscriber.invokeMethodAsync('DreamineVkKeyboardStateChanged', _shiftPressed, _capsLockOn);
        } catch (_) {
            _keyboardSubscribers.delete(subscriber);
        }
    }
}

function updateKeyboardState(ev) {
    if (!ev) return;
    const nextShift = !!ev.shiftKey;
    let nextCaps = _capsLockOn;
    try {
        nextCaps = !!ev.getModifierState?.('CapsLock');
    } catch (_) { }

    if (nextShift !== _shiftPressed || nextCaps !== _capsLockOn) {
        _shiftPressed = nextShift;
        _capsLockOn = nextCaps;
        notifyKeyboardState();
    }
}

document.addEventListener('keydown', updateKeyboardState, true);
document.addEventListener('keyup', updateKeyboardState, true);
document.addEventListener('mousedown', updateKeyboardState, true);
document.addEventListener('pointerdown', updateKeyboardState, true);

// -- Caret guard ------------------------------------------------------------
// After we set input.value + dispatch 'input', Blazor Server round-trips
// through SignalR and eventually re-sets input.value via its diff, which
// causes the browser to jump the caret to the end. The round-trip time
// depends on the network so setTimeout-based retries are fragile.
// Instead we listen to selectionchange and immediately restore the caret
// whenever it drifts away from our desired position. We release the guard
// as soon as the user starts interacting elsewhere, or after 800ms max.
let _guard = null;

function releaseGuard() {
    if (!_guard) return;
    document.removeEventListener('selectionchange', _guard.onSelChange, true);
    document.removeEventListener('mousedown', _guard.onUserAction, true);
    document.removeEventListener('touchstart', _guard.onUserAction, true);
    document.removeEventListener('keydown', _guard.onUserAction, true);
    clearTimeout(_guard.timer);
    _guard = null;
}

function guardCaret(el, pos) {
    releaseGuard();

    const state = { el, pos };

    state.onSelChange = () => {
        if (!el.isConnected) { releaseGuard(); return; }
        if (document.activeElement !== el) return;
        if (el.selectionStart !== pos || el.selectionEnd !== pos) {
            try { el.setSelectionRange(pos, pos); } catch (_) { /* readonly etc */ }
        }
    };

    state.onUserAction = (ev) => {
        // Virtual-keyboard clicks are part of the same edit session. Keep the
        // guard alive so Blazor re-renders cannot move the caret between keys.
        if (isVirtualKeyboardTarget(ev.target)) {
            try {
                el.focus({ preventScroll: true });
                el.setSelectionRange(state.pos, state.pos);
            } catch (_) { }
            return;
        }

        // Real user interaction with the input or elsewhere starts a new
        // selection intent, so release the previous virtual-key edit guard.
        releaseGuard();
    };

    _guard = state;
    document.addEventListener('selectionchange', state.onSelChange, true);
    document.addEventListener('mousedown', state.onUserAction, true);
    document.addEventListener('touchstart', state.onUserAction, true);
    document.addEventListener('keydown', state.onUserAction, true);

    // Safety cap - if selectionchange never fires and no user action happens,
    // release the listeners so we do not pin the caret forever.
    state.timer = setTimeout(releaseGuard, 800);

    // Ensure the caret starts at the desired position.
    try { el.setSelectionRange(pos, pos); } catch (_) { }
}

// -- Public API -------------------------------------------------------------

// Return context of the last focused input, or null.
export function getContext() {
    const el = _lastActive;
    if (!el || !el.isConnected) return null;
    const value = el.value ?? '';
    const guarded = _guard && _guard.el === el && typeof _guard.pos === 'number';
    const start = guarded ? _guard.pos : (typeof el.selectionStart === 'number' ? el.selectionStart : value.length);
    const end = guarded ? _guard.pos : (typeof el.selectionEnd === 'number' ? el.selectionEnd : start);
    return { value, start, end };
}

// Replace [start - replaceCount, end] with insertText, place the caret right after,
// and dispatch 'input' so Blazor's @bind picks up the new value.
// Returns the new caret position, or -1 when no target input is available
// (a plain number keeps the Blazor JSInterop cast simple).
export function applyEdit(replaceCount, insertText) {
    const el = _lastActive;
    if (!el || !el.isConnected) return -1;

    const value = el.value ?? '';
    const start = typeof el.selectionStart === 'number' ? el.selectionStart : value.length;
    const end = typeof el.selectionEnd === 'number' ? el.selectionEnd : start;
    const replaceFrom = Math.max(0, start - Math.max(0, replaceCount || 0));

    const next = value.slice(0, replaceFrom) + (insertText || '') + value.slice(end);
    const nextCaret = replaceFrom + (insertText ? insertText.length : 0);

    try { el.focus({ preventScroll: true }); } catch (_) { }
    el.value = next;
    guardCaret(el, nextCaret);
    el.dispatchEvent(new Event('input', { bubbles: true }));
    try { el.setSelectionRange(nextCaret, nextCaret); } catch (_) { }

    return nextCaret;
}

// Delete the character before the caret (or the selection).
// Returns the new caret position, or -1 when nothing to delete or no target.
export function backspace() {
    const el = _lastActive;
    if (!el || !el.isConnected) return -1;

    const value = el.value ?? '';
    const start = typeof el.selectionStart === 'number' ? el.selectionStart : value.length;
    const end = typeof el.selectionEnd === 'number' ? el.selectionEnd : start;

    let from = start, to = end;
    if (start === end) {
        if (start === 0) return -1;
        from = start - 1;
    }

    const next = value.slice(0, from) + value.slice(to);
    try { el.focus({ preventScroll: true }); } catch (_) { }
    el.value = next;
    guardCaret(el, from);
    el.dispatchEvent(new Event('input', { bubbles: true }));
    try { el.setSelectionRange(from, from); } catch (_) { }
    return from;
}

// Move the caret by delta. Value does not change, but we still guard briefly
// in case another re-render is in flight.
// Returns the new caret position, or -1 when no target.
export function moveCaret(delta) {
    const el = _lastActive;
    if (!el || !el.isConnected) return -1;

    const value = el.value ?? '';
    const start = typeof el.selectionStart === 'number' ? el.selectionStart : value.length;
    const next = Math.max(0, Math.min(value.length, start + (delta | 0)));
    guardCaret(el, next);
    return next;
}

// Re-focus the tracked input (explicit external use).
export function refocus() {
    if (_lastActive && _lastActive.isConnected) {
        _lastActive.focus();
    }
}

export function subscribeKeyboardState(dotnetRef) {
    if (!dotnetRef) return;
    _keyboardSubscribers.add(dotnetRef);
    try {
        dotnetRef.invokeMethodAsync('DreamineVkKeyboardStateChanged', _shiftPressed, _capsLockOn);
    } catch (_) {
        _keyboardSubscribers.delete(dotnetRef);
    }
}

export function unsubscribeKeyboardState(dotnetRef) {
    if (!dotnetRef) return;
    _keyboardSubscribers.delete(dotnetRef);
}

export function getKeyboardState() {
    return { shift: _shiftPressed, capsLock: _capsLockOn };
}
