# Security

## Supported versions

The latest release is the one that gets fixes. Older versions are not patched — the packages ship
together and always carry the same version, so upgrading means moving all of them.

## Reporting a vulnerability

Report privately through
[GitHub's advisory form](https://github.com/The1fEst/Arlecchino/security/advisories/new) rather than
in a public issue. A report is more useful with the version, the platform and terminal it happened
on, and something that reproduces it.

## What is worth reporting

Arlecchino draws to a terminal and reads keys from it, so the interesting surface is what an
application feeds it and what a terminal sends back:

- Text that reaches the screen through `Surface` and escapes the frame — a sequence in application
  data that the renderer passes through rather than measuring as text, and which then moves the
  cursor, changes modes, or writes outside its region.
- Input the terminal sends that the parser mishandles: an escape sequence, a bracketed paste, or a
  mouse report that leaves the router in a state where subsequent keys are read as something else.
- `CopyToClipboard`, which reaches the clipboard of the machine the user is sitting at through
  `OSC 52`, including over a remote session.
- Anything in the source generator that emits code from a name an application controls.

Being able to draw an unpleasant frame is not a vulnerability by itself; escaping the frame or the
terminal's own modes is.
