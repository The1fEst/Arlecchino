# Changelog

Notable changes to the `Arlecchino`, `Arlecchino.Core` and `Arlecchino.Testing` packages. The three ship
together and always carry the same version.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). Up to `1.0.0` a breaking change only
bumped the minor, which is why the `0.x` entries below are full of them; from `1.0.0` on, breaking
the public API means a new major. See
[Versioning](https://the1fest.github.io/Arlecchino.Docs/docs/packages-and-building).

## 6.0.0

A release about where the cursor is. A focus ring is focusable itself, so `Tab` walks into a widget
made of parts and out the far side; the hints box lists the keys of whatever holds the focus rather
than the same keys wherever the cursor stands; and a click reaches the pane it landed in instead of
being offered to every widget in turn. The break is the notification, which stops being a positional
record: the two properties that shadowed each other settle into one, and what is left is named after
what it holds.

### Added

- **The terminal can be lent to another program.** An editor, a pager or a shell cannot share a
  terminal with a full-screen application, so `Handover` stops being one for as long as the other
  program runs: the thread reading keys is parked, the modes are given back, the program is started
  with all three streams its own, and the next frame is drawn whole over whatever it left behind.

  ```csharp
  var code = handover.Run(new ProcessStartInfo("vim") { ArgumentList = { path } });
  ```

  `Give` does the same for work that is not a process. Both are called on the drawing thread and both
  block it, which is the point: nothing is drawn while somebody else has the screen. The terminal comes
  back however the work ended, so a program that could not be started is a message rather than a
  terminal nobody can type in.

- **An application can draw the keys itself.** `ArlecchinoOptions.Hints` replaces `ShowHints` with three
  answers rather than two — `Always`, `WhileWaiting`, `Never` — and `CommandKeys` now says publicly
  whether a chord is half typed and what would finish it. Together they let an application whose screens
  have no borders draw that list in its own shape instead of taking the framework's box:

  ```csharp
  options.Hints = HintsShown.Never;
  // ...
  if (keys.IsWaiting) { DrawTheKeysMyOwnWay(keys.Hints()); }
  ```

  `Never` is a promise taken on rather than a box switched off: a leader with nothing on screen to say
  what is behind it is a key nobody presses twice, which is why the box appeared for a chord even when
  `ShowHints` was false.

- **A key can be a character rather than a key.** `new KeyBinding('!')` answers wherever that character
  can be typed, forgives the Shift held to type it, and writes itself on the key screen as `!`.
  Punctuation had no dependable way to be named before: half of it has no `ConsoleKey`, the half that
  does is named for a US keyboard, and consoles disagree about whether Shift is reported with it. A key
  that could not be named could not be listed either, so the keys a person actually presses were missing
  from the one screen that exists to list them.

- **A click goes to the pane it landed in.** A tree already works out which pane owns which cells in
  order to draw them, and the same knowledge tells a click where to go. A view stops offering the
  event to every widget on the screen in turn, and stops asking each one to guess whether the point
  was its own:

  ```csharp
  public ViewRoute HandleMouse(MouseEvent mouse) => _layout.HandleMouse(mouse);
  ```

  The pane that claims the click takes the focus with it, for a tree that built its ring with
  `AsFocusRing`. A click in the gap between panes, in the surrounding space, or before the first frame
  was drawn belongs to no pane and is left alone.

- **A ring inside a ring.** `FocusRing` is an `IArlecchinoFocusable` itself, so one goes inside
  another: add a ring to a ring and `Tab` walks into it, through what it holds and out the far side,
  without the view saying anything about it. A nested ring remembers where it was left, so coming back
  to it from either side lands where the cursor was rather than at the top. `MoveFocus` is what a
  widget made of several fields answers to take a step of its own; leaving it alone says the step was
  not taken, and the ring around it moves on as it always has.

- **The hints box follows the focus.** A screen of unlike panes listed the same keys wherever the
  cursor stood. An element now states its own keys through `IArlecchinoFocusable.Hints`, and a view
  points at whatever holds the focus:

  ```csharp
  public IArlecchinoFocusable Focus => _ring;
  ```

  The keys of the focused element come first and the keys of the screen after them, minus any the
  element already claimed, so one key is not listed twice under two labels.

### Changed

- **A notification is a class, and its members are named after what they hold.** The type was a
  positional record, which is why the value that changes when work ends had to live beside the one it
  shadows: `Level` was fixed at the moment of raising, so what the entry turned out to be was read from
  a second property, `Loudness`. A class settles both into one `Level` a caller reads and the framework
  writes. `Time` goes the same way, into the `Since` that was already keeping the moment the timeouts
  are counted from. What is left is renamed for what it is: `Progress` was the line of text and `Share`
  the fraction, so the text is `ProgressText` and the fraction is `Progress`; `Ended` is `EndedText`,
  beside the `Line` it stands in for.

  ```csharp
  state.Notifications.Raise(new(DateTimeOffset.Now, NotificationLevel.Information, "copying")
  {
      ProgressText = () => $"copied {done} of {all}",
      Progress = () => (double)done / all,
  });
  ```

  Being a record was never used for what a record is for: entries are held by reference and settled in
  place, so value equality, `with` and `Deconstruct` had nothing to do. They are gone with it.

### Fixed

- **A paste into a Windows console no longer runs what was pasted.** A terminal elsewhere wraps pasted
  text in `ESC[200~` and `ESC[201~`, so the newline at the end of a pasted command is text; the console
  reports the paste as the keys that would have typed it, and that last newline arrived as `Enter`. A
  run of characters already waiting together is now read as a paste and wrapped in those same markers,
  so it reaches the application as pasted text. A newline in the run settles it, and without one it
  takes four characters — two keys that landed in the same read are still typing, and a run of one
  character is a held key repeating.
- **A key Windows types with Shift now reads the way it reads everywhere else.** The console reports
  Shift alongside the character it typed, so `:` arrived as `Oem1` with Shift held while every other
  platform sends a colon and no modifier at all — and anything written against that, a binding on the
  character or a screen that answers to one, did nothing on Windows. Shift is now dropped where it did
  nothing but type the character the event already carries. Shift on a key that types nothing — a
  function key, Tab, Insert — is a modifier in its own right and is kept.

## 5.0.0

A release about the keyboard. A key press stops being the console's own type and becomes `KeyPress`, which
has room for the modifier that type could not hold: Command, which a terminal does report and the console
type could only misread. A binding stops being a list of positions and is built instead — alternatives
added to it one at a time, and a second keystroke turning it into a chord, which is how an application
reaches past the handful of combinations a terminal will actually hand on. The break is the type itself —
a handler takes `KeyPress` and a binding matches one, so `ConsoleKeyInfo` in a signature is what a build
fails on, and what it is replaced with is spelled the same way.

### Added

- **Command, and a way to move a whole keymap onto it.** On a Mac terminal the Option key is spoken
  for by the characters it types, so `Alt` never reaches the application and every binding built on it
  is unreachable. The key that keyboard has going spare is Command, and terminals do report it — as one
  more bit in the same modifier field as the rest. `KeyModifiers.Super` is that bit, bindable like any
  other:

  ```csharp
  builder.UseKeymap(new ArlecchinoKeymap { Back = new(ConsoleKey.LeftArrow, KeyModifiers.Super) });
  ```

  Rewriting thirty bindings by hand is how an application ends up with twenty-eight of them rewritten,
  so the whole map moves at once:

  ```csharp
  builder.UseKeymap(new ArlecchinoKeymap().Replacing(KeyModifiers.Alt, KeyModifiers.Super));
  ```

  A binding relabels itself for the machine it is running on: `Cmd+←` on a Mac, `Win+←` elsewhere.

- **A binding of two keystrokes.** `ThenKey` finishes a binding with a second key, pressed after the
  first is let go, and the pair is one command:

  ```csharp
  new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);
  ```

  This is how an application gets past the modifiers a terminal will give it. Option is spoken for by
  the characters it types and Command belongs to the window, so what is left on a Mac are the letters
  held with Control, and there are not thirty of those. A leader spends one of them and hands back the
  alphabet behind it. While a leader is half typed, the hints box stops listing the keys that are out
  of reach and lists what finishes the chord instead, so the second key is read rather than remembered.
  `Opens` and `Closes` ask about the two halves; `Matches` answers `false` for a chord, so a leader on
  its own runs nothing. An application that turned the hints box off still gets this one: turning it
  off says something about the keys of a screen, not about a key half pressed.

### Changed

- **A key press is `KeyPress` rather than `ConsoleKeyInfo`.** The console type stores Shift, Alt and
  Control as three booleans and has nowhere to put a fourth modifier, so a terminal reporting Command
  could only be misread or dropped. Everything a view is handed goes through the new type instead:
  `Handle(KeyPress key)` on views, widgets, focusables and dialogs, `KeyBinding.Matches`,
  `IArlecchinoTerminal.ReadKey`, `KeyText.Resolve` and `MouseEvent.Modifiers`. The members are the same
  three, one of them renamed: `Key`, `Modifiers` and `Character` where the console said `KeyChar`.
  A view is fixed by changing the parameter type; `KeyPress.From` takes over a `ConsoleKeyInfo` for
  code that still has one.
- **A binding is built rather than listed.** `KeyBinding` carried its second combination as two more
  positional parameters, `AlsoKey` and `AlsoModifiers`, which allowed exactly one of them and read as
  four keys in a row at the call site. Both are gone. A binding is now the combination it is named
  after, and everything else is added to it:

  ```csharp
  new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
      .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);
  ```

  `AddAlternative` takes as many as there are habits to answer, and the combinations it adds are
  matched but never written, since a binding is shown under one name. The one it is written from is
  `First`, the rest are `Alternatives`, and both are the new `KeyStroke` — a key and the modifiers held
  with it, which is also what a chord's second half is.

  An alternative is one press even where the binding is a chord, which is how a chord reaches a
  keyboard the other way round — `Ctrl+G U` on the laptop that has to spell it out, `Ctrl+PgUp` on the
  keyboard with the key:

  ```csharp
  new KeyBinding(ConsoleKey.G, KeyModifiers.Control)
      .ThenKey(ConsoleKey.U)
      .AddAlternative(ConsoleKey.PageUp, KeyModifiers.Control);
  ```
- **The testing helpers take modifiers rather than three booleans.** `ArlecchinoTestHost.Press`,
  `TestApplication.Press` and `SessionTape.Key` read `Press(ConsoleKey.C, KeyModifiers.Super)`, which
  is also the only way to press the modifier the booleans had no room for. A tape written by an older
  version does not load: the key line now carries one number for the modifiers instead of three flags.

### Fixed

- **A letter held with Command is no longer typed into the field as text.** There is no legacy spelling
  for `Cmd+J`, so a terminal falls back to the `CSI 106;9u` shape — which the reader did not understand
  and therefore replayed a character at a time, putting `[106;9u` into whatever was being edited. That
  shape is now read. Keys named in the private use area, which is where a terminal puts the keypad and
  the media keys, are understood and dropped rather than replayed.
- **A cursor key held with Command is no longer indistinguishable from the bare key.** `Cmd+←` arrives
  as `ESC[1;9D`, and the ninth bit was being dropped on the floor, leaving the press to read as a plain
  `←` and move the cursor.
- **Keys pressed while the terminal is being asked what it can do no longer disappear.** The probe
  hands back what it read only when the terminal said nothing at all, so anything typed in the moment
  before an answer arrived was filed as part of that answer and lost. It is now told apart by where it
  sits: every answer a terminal gives is an escape sequence, so what lands outside one was typed and
  goes back to the application. An application that starts fast enough to be typed at during its first
  frame keeps those keystrokes.

## 4.0.0

A release about names and about frames. Text gets a name the compiler checks instead of being typed
twice; a dialog, a list row and the whole screen around a view become things an application writes for
itself rather than things the framework decides. The one break is namespaces — nothing was renamed or
removed, and a build that fails on it is fixed by adding a sub-namespace to a `using`. See
[Migrating to 4.0](https://the1fest.github.io/Arlecchino.Docs/docs/migrating-to-4.0).

### Added

- **A localization generator.** Text written where it is drawn gets written twice — the same sentence
  in a dialog and in the log that follows it — and the day one of them is reworded the two quietly
  disagree. Put the text in a TOML file instead and the generator emits a `LocString` name for each
  entry and a `Loc` that resolves it, so the second mention is a name the compiler checks rather than
  a sentence somebody retyped. Translation comes free of the same machinery, but one language is
  reason enough:

  ```toml
  # Localization/Localization.toml
  [localization]
  language = "en"

  [strings]
  Copy = "Copy"
  CopyManyTitle = "Copy {0} items"
  ```

  ```xml
  <ItemGroup>
    <AdditionalFiles Include="Localization\*.toml" />
  </ItemGroup>
  ```

  ```csharp
  Title = sources.Count == 1 ? Loc(LocString.Copy) : Loc(LocString.CopyManyTitle, sources.Count);
  ```

  Every other file in the folder is a translation of the default; a string it leaves out falls back to
  the default rather than leaving a hole on the screen, and one it invents is an error. The folder and
  the default language are `ArlecchinoLocalizationFolder` and `ArlecchinoLocalizationLanguage`.

  It writes **`Bind`** alongside them, which is a key named out of the same file:

  ```csharp
  Bind.To(new(ConsoleKey.F5), LocString.Copy, files.Copy)
  Bind.Going(new(ConsoleKey.F3), LocString.View, files.Read)
  Bind.When(new(ConsoleKey.Escape, ConsoleModifiers.Alt), LocString.Stop, () => work.IsBusy, work.Cancel)
  ```

  `ViewCommand` takes a `Func<string>` and can take nothing else — a label is read every frame so that
  changing language changes the screen — and it cannot take a `LocString`, because there is no such
  type until an application is compiled. So the shorthand is written into the application beside the
  enum it names, which is the only place both are in scope.

- **A dialog of your own.** The framework's own dialogs know what a number looks like and what a choice
  looks like; an application with a look of its own wants neither. Derive from `Modal` — the same slot,
  the same stack and the same rules as every dialog the framework brings, with the drawing and the keys
  yours:

  ```csharp
  public sealed class ConfirmModal : Modal
  {
      public bool Answered { get; private set; }

      public override void Draw(ModalFrame frame) =>
          frame.Screen.Rows(2, 1).WriteLine(0, "Really?", Theme.Warning, Align.Center);

      public override void Handle(ModalFrame frame, ConsoleKeyInfo key)
      {
          Answered = key.Key == ConsoleKey.Y;
          frame.Close();
      }
  }

  state.Modal = new ConfirmModal { Title = "Careful" };
  ```

  A dialog is a value, so it cannot be handed services when it is built. **`ModalFrame`** carries them
  instead, for as long as the dialog is on screen: where to draw, the words, the keys to obey, `Close`,
  `Copy`, and `Box` — the titled box with its hints under a rule that every dialog the framework brings
  is drawn through, so one you write reads as the same application.

- **`ListBox<T>.PaintRow`**, for a list whose rows are not one colour. `Render` and `ItemStyle` write a
  row as one string in one style, which is right for most lists and wrong for any where a name, a size
  and a date each want their own. Set this instead and the list hands over one row of itself to draw
  in, having already worked out the scrolling, the wheel and the clicks:

  ```csharp
  var files = new ListBox<FileEntry>(keymap)
  {
      Render = static entry => entry.Name,
      PaintRow = (row, entry, chosen) =>
      {
          row.Fill(chosen ? Theme.ActiveSelected : Theme.Default);
          row.Write(0, 0, entry.Name, chosen ? Theme.ActiveSelected : Theme.Accent);
          row.WriteLine(0, Sizes.Brief(entry.Size), Theme.Muted, Align.Right);
      },
  };
  ```

- **`IArlecchinoLayout`**, the frame every view is drawn inside — a band along the top, a bar along the
  bottom, whatever a screen of this application always has around it. It is Razor's `_Layout` with
  `@RenderBody()`: the layout is handed the room there is and a delegate that draws the view, and where
  it calls that delegate is where the view goes.

  ```csharp
  public sealed class Chrome : IArlecchinoLayout
  {
      public void Draw(SurfaceRegion frame, Action<SurfaceRegion> body)
      {
          _tabs.Draw(frame.Rows(0, 1));
          body(frame.Rows(1, frame.Height - 2));
          _bar.Draw(frame.Rows(frame.Height - 1, 1));
      }
  }

  builder.Services.AddArlecchino().UseLayout<Chrome>();
  ```

  One instance serves the whole application, so what it holds outlives the view — a row of tabs keeps
  its scroll position when a screen is left and come back to, which is the point of having a header in
  one place rather than drawn again by every view. `Surface.Content` answers with the room the layout
  left, so **no view has to be edited**: it asks for its content as it always did and is handed what it
  was given. A screen that wants the whole terminal answers `false` to `IArlecchinoView.UsesLayout`.

  `IArlecchinoLayout.HandleMouse` sees a click before the view does, for a header that answers to one.
  There is no key equivalent on purpose: a key that works on every screen is an `IArlecchinoCommand`,
  which the framework already had.

- **`Notifications.Recent`**, everything worth showing right now rather than only the newest line.
  `Current` answers for one row at the bottom of the screen, and one row can hold one message; an
  application that shows its work as a stack of cards in the corner wants all of it. This is everything
  still running whatever its age, plus everything that ended within `NotificationTimeout`, newest
  first — so a copy that takes an hour stays up for the hour rather than timing out while it works:

  ```csharp
  foreach (var entry in state.Notifications.Recent)
  {
      card.WriteLine(0, entry.Line, entry.Loudness == NotificationLevel.Failure ? Theme.Error : Theme.Default);
  }
  ```

### Changed

- **A `ViewCommand` that is disabled no longer swallows its key.** `IsEnabled` used to mean two things
  at once — greyed out on the key screen, and a key that silently does nothing — and the second one
  left a view unable to give the key a second meaning for exactly the times its command is off. Now an
  unavailable command is skipped and the key carries on to the commands available everywhere and then
  to the view's own `Handle`, as if nothing had claimed it:

  ```csharp
  new ViewCommand
  {
      Binding = new(ConsoleKey.Escape),
      Label = () => "stop what is running",
      IsEnabled = () => operations.IsBusy,
      Run = () => { operations.Cancel(); return ViewRoute.None; },
  }
  ```

  With nothing running, Escape now reaches the view — to leave a search, to clear a filter — instead
  of disappearing. An application that relied on the key being eaten should bind it and return
  `ViewRoute.None` rather than disable it.

- **Namespaces now follow the folders they are in**, which is the whole of the break in this major.
  Four folders had grown past the point where a name could be found in them by looking, so each was
  split by what its files are for, and the namespaces went with them:

  | Was | Now |
  | --- | --- |
  | `Arlecchino.Modals` | `.Asking` (text, number), `.Choosing` (choice, palette), `.Setting` (slider, toggle, colour, date, time), `.Telling` (message, notification) |
  | `Arlecchino.Widgets` | `.Lists` (list, table, tree, tabs, scrolling), `.Pictures`, `.Readouts` (charts, indicators, status bar, text view) |
  | `Arlecchino.Rendering` | `.Colors` (theme, palette, colour types), `.Text` (widths, joinery, symbols), `.Terminals` (capabilities, probe, image protocol) |
  | `Arlecchino.Atoms` | `.Local`, `.Tracked`, `.Collections` |

  `Modal`, `ModalFrame`, `Surface`, `SurfaceRegion`, `Margin`, `Align`, `Atom` and the store
  interfaces stay where they were: they are the vocabulary every file already reaches for. Nothing was
  renamed and nothing was removed — a build that fails on this is fixed by adding the sub-namespace to
  a `using`.

### Fixed

- **`Alt+Esc` reached an application as two plain Escapes**, which left it impossible to bind. Holding
  Alt puts an escape in front of the key, so this one is two of them; the runtime folds that prefix
  back for every other key — `\ea` arrives as `Alt+A` — and leaves `\e\e` as it found it. The reader
  folds it now.

  There is no such fix for `Ctrl+Esc`, and there cannot be one: a terminal has no encoding for it in
  the sequences everything speaks today, so nothing is sent at all. Reach for `Alt+Esc` instead, or
  wait for the kitty keyboard protocol.

## 3.1.0

Everything here is in `Arlecchino.Testing`. A test used to read the bytes a frame wrote; now it reads
the screen those bytes left, and the two are not the same thing — frames are written as the difference
from the last one.

### Added

- **`ScreenGrid`**, the screen a terminal would be holding rather than the bytes that got it there.
  Where `FrameText` strips escapes out of what was written, this obeys them: a cursor jump moves the
  cursor, a style sticks to the cells that follow, a wide symbol takes two columns, and a graphics
  payload is stepped over instead of being spelled out.

  ```csharp
  app.Press(ConsoleKey.DownArrow);
  app.Frame();

  Assert.Equal("Widebody kit", app.Screen.Line(3).Trim());
  Assert.Equal(Theme.Selected.Ansi, app.Screen.StyleAt(3, 2));
  ```

  It reaches a test as `ArlecchinoTestHost.Screen` and `FakeTerminal.Screen`, and it survives
  `Clear()` the way a real screen survives forgetting what you typed. `Matches` compares two screens by
  symbol and by style, `CursorRow` and `CursorColumn` say where the cursor was left, and `Apply` and
  `Resize` are there for a test driving it directly.

  The screen this emulates was held against real terminals rather than against its author's idea of
  one: frames are played into a `tmux` pane and compared cell by cell, colour and all, and the corners
  where terminals disagree with each other are named in the documentation instead of being guessed at.

### Changed

- **Every frame drawn against a `FakeTerminal` is held against the frame that was composed**, cell by
  cell, symbol and style. A difference means the writing left something on screen that the drawing did
  not have, and the frame throws with both pictures in the message.

  This is the whole reason the screen exists. An idle frame writes nothing and a frame that changed one
  cell writes one cell, so what was written says nothing about what is on screen — and a test that only
  ever asks what was written cannot see a difference that failed to go out. **A widget of your own that
  draws outside its region, or draws differently the second time, will now fail whichever test happens
  to draw it twice.** It costs a pass over the cells and no second render.

- **`ArlecchinoTestHost.Frame()` draws the way a running application does** — as the difference from
  the last frame — and returns the screen afterwards rather than the text of what went out. A test that
  draws a single frame reads exactly what it read before; a test that draws several now reads the whole
  picture instead of the handful of cells that changed. `Styles()` still draws whole frames, because a
  diffed frame only restates the colours of the cells it rewrote.

- **`FakeTerminal.EnqueueText` names the key where a console names it.** Enter, Tab, Backspace, the
  space bar, a letter, a digit and a control chord now arrive carrying their `ConsoleKey`, because that
  is what `Console.ReadKey` hands an application; they used to arrive as a bare character with no key,
  which is a shape no terminal produces. Escape sequences still arrive a character at a time — the
  other shape a console produces, and the one the reader has to make sense of on its own.

  This was found by pressing every key in a real terminal and reading the bytes off the pty. A test
  asserting that a typed `'a'` carries no key needs updating; nothing in this repository did.

## 3.0.0

### Added

- **`Joinery`**, lines that know about one another. `region.Border(...)` draws a box that knows
  nothing of its neighbours, so two panes that touch put two verticals where the eye expects one.
  Boxes and rules recorded here are painted at the end, and a shared cell becomes the glyph that
  joins them:

  ```csharp
  var joinery = new Joinery();

  var files = joinery.Box(left, Theme.Info, "files");
  var log = joinery.Box(right, Theme.Active, "log");

  joinery.Draw(surface.Content, Theme.Info);
  ```

  ```text
  ╭─ one ───────────────┬─ three ──────────────╮
  ├─ two ───────────────┼─ four ───────────────┤
  ╰─────────────────────┴──────────────────────╯
  ```

  `Box` hands back the room inside, as `Border` does, and `Across` and `Down` record rules that join
  whatever they meet. Coordinates are the surface's own, so regions from anywhere on the frame are
  recorded together; a cell takes the style of the last thing recorded over it, which is how the pane
  holding the focus wins the edges it shares.

- **`Surface.Passthrough(row, column, payload)`**, for what the cell grid cannot express — an image in
  one of the terminal graphics protocols. It goes out after the cells, so whatever was under or around
  it is repainted before it lands and a payload that stops being handed over disappears as the cells
  beneath it are drawn again; and it is not sent while it stays the same, which matters when a picture
  weighs kilobytes.
- **`Picture`**, an image drawn in cells: each one carries two pixels, painted as the colour of the
  upper half block and the background behind it, so a cell that is twice as tall as it is wide comes
  out roughly square per pixel.

  ```csharp
  private readonly Picture _preview = new() { Background = Theme.Default };

  _preview.Show(pixels, width, height);
  _preview.Draw(region);
  ```

  It asks nothing of the terminal but the colour it already draws in — no image protocol, no state
  left behind, nothing to clean up when the picture goes away — so it works everywhere the framework
  works and degrades with the palette like any other colour. The picture is fitted without stretching
  and centred, and one smaller than its pane is enlarged, so an icon is visible rather than a speck.

  Pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the
  application, which knows what it is willing to depend on, while the framework draws what it is
  given.
- **The terminal is asked what it can do**, once, as the application starts: which graphics protocols it
  speaks and how many pixels a cell is. `ImageProtocol.Auto` is the new default, so a picture is drawn
  with the best of what the terminal admitted to — kitty, else sixel, else cells — instead of with a
  protocol an application had to guess at.

  ```csharp
  TerminalCapabilities.Sixel          // what it said
  TerminalCapabilities.Kitty
  TerminalCapabilities.CellSizeKnown  // whether the size below was reported or guessed
  Glyphs.CellWidth
  ```

  `CellSizeKnown` earns its place: ten by twenty is both the standing guess and what a terminal at a
  common font size actually reports, so without it there is no telling the two apart — and sixel sizing
  rests on the difference.

  The whole thing turns on one arrangement: the questions go out in an order ending with primary device
  attributes, which every terminal answers, so **that** reply is the signal no other reply is coming.
  Without a fence like it there is nothing to wait on but a guess at how long silence takes.

  Nothing typed is swallowed. Every answer opens with an escape, so the first character that is not one
  ends the wait and goes back to the terminal through the new `IArlecchinoTerminal.Unread`. A terminal
  that says nothing at all costs `ArlecchinoOptions.TerminalAnswer` — 120 ms by default — once, and
  leaves every setting alone; `AskTerminal = false` skips even that.
- **`IArlecchinoTerminal.Unread(key)`**, which puts a key back so the next read returns it and
  `KeyAvailable` reports it. A custom terminal has to implement it — a queue in front of the real read is
  the whole of it. It exists because code that must read a key to discover it did not want it should hand
  it back to the terminal, rather than make its caller carry it.
- **Sixel**, which is what Windows Terminal, xterm and foot speak: `ImageProtocol.Sixel` puts the
  pixels out in bands of six rows with runs collapsed, without which a photograph would weigh several
  times what it needs to.

  The format draws from colour registers, so the picture is brought down to a palette of at most 256.
  That palette is the picture's own: colours are gathered into boxes and the widest box is split at its
  weighted median until the registers run out, which spends them where the picture actually has detail.
  Shrinking averages every source pixel a destination pixel covers rather than picking one of them. On
  the project's own social card reduced to sixty columns, the two together bring the mean error per
  channel from 22.4 to 1.2 and the worst from 191 to 6 against a fixed cube and nearest-pixel sampling.

  Sixel is measured in pixels and knows nothing of cells, so a picture is resampled to however many
  pixels the cells it was given come to, and `Glyphs.CellWidth` and `Glyphs.CellHeight` say how large
  a cell is taken to be — they also set the shape of a cell, so a picture keeps its proportions. There
  is no asking the terminal yet: ten by twenty is the guess, a wrong one shows as a picture that does
  not quite fill its pane, and an application that knows better can say so.

  A payload is built once and kept until the picture, the protocol or the room it is drawn in changes,
  since choosing a palette is real work and a frame asks for the same bytes again.
- **`Surface.Passthrough` takes what undraws the payload as well as the payload.** A payload that was
  handed over last frame and is not handed over this one — the widget moved, or shrank, or its screen is
  not on show any more — has its undraw written where it used to be, before anything new is written.

  Repainting the cells was supposed to be enough. It is not: a sixel that a view stopped drawing stayed on
  the screen, over whatever was drawn next, because the widget that could have removed it was no longer
  being asked to draw at all. Only the surface knows a payload disappeared, and only whoever sent it knows
  how to take it back, so the two have to meet in the seam.

  The undraw goes out **before** the frame's cells, and a frame that undraws anything is written whole
  rather than diffed. Both follow from what an undraw is: painting over. Written after the cells it paints
  over them, which erases the frame instead of the picture; and the cells it painted over have to be put
  back whether the diff thinks they changed or not.
- **A picture is undrawn when it is cleared.** Every `Picture` owns a kitty image number, so new pixels
  replace the image the terminal is holding rather than adding another one, and `Clear()` deletes it. What
  was there before handed the terminal an image per change and never took one back — a picture updated on
  a timer grew the terminal's memory for as long as the session lasted.

  Sixel has nothing to delete: it writes pixels into the screen rather than into a registry of images, so
  undrawing means painting over them, and painting needs a colour. That is the fifth thing the terminal is
  asked for — `OSC 11`, the colour behind its text — and it lands in `TerminalCapabilities.Background`. A
  terminal that will not say leaves the pixels where they are on purpose: a guessed colour paints a
  rectangle anyone can see, which is worse than the leftover it was meant to remove.

  Whether the leftover shows at all is a matter of terminal: Windows Terminal ties image data to the cell
  and drops it when the cell is written, xterm keeps sixel in a layer text does not disturb. Neither
  behaviour is written down anywhere to rely on, which is why this does not lean on either.
- **The kitty graphics protocol**, where the terminal speaks it: `ImageProtocol.Kitty` sends the
  pixels themselves instead of cells, and the picture is as sharp as the screen allows.

  ```csharp
  services.AddArlecchino(options => options.ImageProtocol = ImageProtocol.Kitty);

  Glyphs.Picture = ImageProtocol.Blocks;      // later, from a settings screen
  _preview.Protocol = ImageProtocol.Kitty;    // or for one pane
  ```

  Cells stay the default: a terminal that cannot speak the protocol would print the escape sequence
  as text, so this is asked for rather than assumed. Replies are suppressed with `q=2`, since a
  terminal answering would reach the input reader as a stray escape sequence, and a payload measured
  in kilobytes is only re-sent when the picture or its placement changed.
- **`SessionTape`** in `Arlecchino.Testing`, which writes a test as the session it describes rather
  than as a dozen calls with the assertions lost among them:

  ```csharp
  var frames = new SessionTape()
      .Type(":")
      .Shot()
      .Type("copy")
      .Wait(200)
      .Shot()
      .Play(host);

  Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);
  ```

  A tape is text, one step to a line, so `Read` takes back what `ToString` wrote and a session travels
  as a file. Waits are part of it, which is what makes timeouts and work on a clock replayable, and
  every mark hands back a frame. `ArlecchinoTestHost` gained `Send(ConsoleKeyInfo)`, `Send(MouseEvent)`
  and `SendPaste` for feeding an event exactly as a terminal reports one.

  It records nothing on its own. Capturing a running application was considered and dropped: the
  framework has a password modal and a paste step, so a tape from a real session would hold whatever
  was typed into them, and no application should write a file like that on a user's behalf.

### Changed

- **Notifications are state.** `Notifications` held a plain `List<Notification>` and asked for a
  repaint by hand, which meant two things: an entry falling out on its timeout changed nothing on
  screen until something else asked for a frame, and `Raise` from a background task corrupted the
  list quietly. It now holds a `LocalAtomsList<Notification>`, so every change asks for a frame by
  itself — **and raising one from a thread that is not the drawing thread throws** instead of being
  tolerated. Work that reports from the background hands it over:

  ```csharp
  FrameThread.Post(() => state.Notifications.Notify("done"));
  ```

  This is the reason the change waited for a major: an application that has been raising
  notifications from a worker has been getting away with it.
- **The rest of `ArlecchinoState` is checked too.** `Output` asked which thread it was on; `Modal`,
  `PushModal`, `CloseModal`, `CloseAllModals`, `FilePicker` and `PickerLastFolder` did not, so a
  background task could open a dialog halfway through a frame — into a surface already measured
  without it — and the `Request…` helpers inherited the hole, since each of them assigns `Modal`.
  **All of them now throw off the drawing thread**, and the way through is the same as everywhere
  else:

  ```csharp
  FrameThread.Post(() => state.RequestMessage("Done", "The upload finished."));
  ```

  `Invalidate` is still callable from anywhere: asking for a frame is what a background thread is
  supposed to do. Same reason for the major as the notifications entry above — code that has been
  opening dialogs from a worker has been getting away with it.
- **The stack of dialogs is state too.** It was a plain `List<Modal>` with a `_repaint.Request()` after
  every mutation — the same shape the notifications had, and the same way to get it wrong: a new way to
  open or close one is a frame nobody asked for. It is now a `LocalAtomsList<Modal>`, so the list asks
  by itself, and a close that closes nothing asks for nothing.

  `Modals` still hands out `IReadOnlyList<Modal>` and is still a live view, so nothing an application
  reads changes. The list is deliberately outside the undo history: stepping back through what was typed
  should not reopen a dialog that was answered.
- **The look is state too.** `Theme.Palette`, `Glyphs.Graph`, `Glyphs.Picture`, `Glyphs.CellWidth` and
  `Glyphs.CellHeight` were plain settable statics: a frame reads every one of them, and a background
  thread could swap the palette or the protocol halfway through drawing one. They are checked now, the
  way `ArlecchinoState` is, and each **asks for a frame by itself** — the doc used to tell you to call
  `Repaint.Request()` after changing them, and that is no longer needed.

  ```csharp
  FrameThread.Post(() => Glyphs.Picture = ImageProtocol.Sixel);   // from a worker
  ```

  The look the options asked for is now installed while `AddArlecchino` runs rather than when the
  container first hands the options out. A container resolves on whichever thread asked first, so the
  old timing could have installed a palette from a thread that was not drawing — the checks made that
  visible.
- **`ArlecchinoOptions.CellWidth` and `CellHeight`**, so the size a cell is taken to be can be
  configured like its two neighbours instead of only through the static. Sixel is the one that reads
  them.
- **Any language can be typed without asking.** `TextInputMode.Native` is the default, so a
  non-Latin layout works out of the box.
- **The other mode is named for what it does, and now does it without exception.** What was
  `UseLatinOnlyInput()` is `UseKeysByPosition()`: every character comes from where its key sits on
  the keyboard rather than from what the layout makes of it, so the key left of `S` types `a` whether
  the layout says `a`, `ф` or `α`. It used to make an exception for characters that were already
  ASCII, which meant a layout that moves letters around was read inconsistently; the position now
  decides on its own. The price is unchanged and worth stating plainly: in this mode those languages
  cannot be typed at all.

  ```csharp
  builder.UseKeysByPosition();
  ```

- **A `PaneTree` with no gap draws one line between its panes, not two.** Titled panes went through
  `SurfaceRegion.Border`, so `Gaps(inner: 0)` put `╮╭` where the eye expects `┬`. The tree now records
  its boxes in a `Joinery` and paints them together, and panes in a box that touch are pulled onto
  one another's edge so the line is shared:

  ```text
  ├─ files ────────────┬─ authors ─────────────┬─ log ────────────┤
  │ Program.cs         │ fEst                  │ the rest of it   │
  ╰────────────────────┴───────────────────────┴──────────────────╯
  ```

  A pane without a box is left where it was, since it would lose a column of what it draws to a
  neighbour's border, and any tree with a gap is unchanged. The pane holding the focus is recorded
  last, so the edge it shares takes its colour rather than its neighbour's — `Tab` still moves a
  highlight around the screen, now along lines that meet.

### Fixed

Found by reading the whole of it rather than by anything failing.

- **A picture could vanish from a frame that was written whole.** Writing every cell is what removes the
  pixels over them in some terminals, and the payload was only re-sent when it had changed — so a frame
  written whole erased the picture and did not put it back. `Surface.ForgetPreviousFrame`, a resize and
  every frame of a fixed-size surface all took that path.
- **A picture drawn in cells was written out again every frame, however still it was.** It built the
  colour of each cell fresh each time, and the frame diff tells cells apart by reference, so nothing ever
  looked unchanged. The colours are worked out when the picture or its room changes and kept between
  frames — which also stops an allocation per cell per frame, and stops the escape sequence for each one
  being rebuilt.
- **Undrawing a sixel could paint below it.** Bands are six rows whatever the picture's height, and the
  last one was painted full, reaching up to five rows past the picture on a terminal that does not clip to
  the raster size.
- **The terminal probe assumed answers came back in the order they were asked for.** Primary device
  attributes are asked last and used as the fence, so a terminal answering it early cut off whatever was
  still coming. The fence now stops the waiting rather than the reading: what is already buffered is taken
  too.
- **A notification settled in place told nothing that watched the list.** `Notification` is mutable on
  purpose — a dialog someone has open changes under them — but writing a property of an item is not a
  change to the list it sits in, so a `Computed` over it never recomputed. `AtomsList.Touch()` says an
  item changed inside itself; `Notifications` no longer needs `Repaint` at all.
- **A console read that failed reached views as a key press of NUL.** `default` is what a failed read
  answers and nothing tells it apart from a real key, so it is dropped where input enters instead.

### Removed

- **`ArlecchinoBuilder.UseNativeInput()`**, which now says what already happens. Delete the call —
  the behaviour it asked for is what an application gets by default.
- **`UseLatinOnlyInput()`, `TextInputMode.LatinOnly` and `KeyText.LatinOnly`**, renamed to
  `UseKeysByPosition()`, `TextInputMode.ByPosition` and `KeyText.ByPosition`. The old names described
  what the mode accepted; the new ones describe how it decides.

## 2.13.0

### Added

- **`AreaChart`**, a series drawn as a filled area over as many rows as it is given — the shape a
  system monitor shows, and the one thing `Sparkline` cannot be. A cell carries two samples side by
  side and several levels of height, so a chart eight rows tall has thirty-two levels between empty
  and full and twice the history of a row of blocks:

  ```csharp
  private readonly AreaChart _cpu = new()
  {
      Values = _history,
      Minimum = 0,
      Maximum = 100,
      Bands = [new(0m, Theme.Active), new(60m, Theme.Warning), new(85m, Theme.Error)],
  };
  ```

  Colour comes from how high the fill climbed rather than from anything the view works out: a
  terminal with truecolor blends between the bands, a 256-colour one quantises that blend, and one
  with no colour draws the shape alone. `Invert` hangs the chart from the top, for the second half of
  a mirrored pair.
- **`GraphSymbols` and `Glyphs.Graph`**, the choice of what graphs are drawn with — `Braille` for
  four levels a cell, `Blocks` for two, `Tty` for a console whose font carries little more than
  ASCII. `ArlecchinoOptions.GraphSymbols` installs it, `Glyphs.Graph` is process-wide and settable
  afterwards, and a widget's own `Symbols` overrides it, so an application can offer the choice in
  its own settings and every chart follows on the next frame.

  It is a font question rather than a platform one: Windows Terminal falls back per glyph and renders
  braille even when the configured font has none of it, while the classic console host does not.

## 2.12.0

### Added

- **`AtomsSet<T>`**, in a tracked and a local flavour, for the state that is a question of in or out
  — the files marked, the rows expanded, the hosts that answered:

  ```csharp
  public TrackedAtomsSet<string> Marked { get; } = new(comparer: StringComparer.OrdinalIgnoreCase);

  Marked.Add(path);
  Marked.Add(everythingBelow);          // one notification and one undo step for the lot

  if (Marked.TryRemove(path))
  {
      Say($"{path} is no longer marked");
  }
  ```

  It follows `HashSet<T>` rather than the map: putting in what is already there is idempotent rather
  than an exception, which is why `Add` answers nothing and `TryAdd` and `TryRemove` are there for
  when the answer matters. `Value` is a live, read-only `IReadOnlySet<T>`, so `Contains`,
  `SetEquals` and `IsSubsetOf` are all reachable without copying anything — the read-only wrapper is
  the framework's own, since `ReadOnlySet<T>` arrived in .NET 9 and the packages still build for
  `net8.0`.

## 2.11.0

### Added

- **`AtomsQueue<T>` and `AtomsStack<T>`**, in a tracked and a local flavour each, which finishes the
  set: what a `ConcurrentQueue<T>` or a `ConcurrentStack<T>` holds belongs in state that notifies,
  asks for a frame and records a step, rather than in a container whose thread safety a single
  drawing thread has no use for.

  ```csharp
  public LocalAtomsQueue<FileEntry> ToCopy { get; } = new();
  public LocalAtomsStack<string> Been { get; } = new();

  ToCopy.Enqueue(entries);
  Been.Push(folder);

  if (ToCopy.TryDequeue(out var next))
  {
      Copy(next);
  }
  ```

  `Dequeue`, `Pop` and `Peek` throw on an empty one as the collections they are named after do, and
  `TryDequeue`, `TryPop` and `TryPeek` answer instead. `Value` reads front first for a queue and top
  first for a stack — the order `Stack<T>` itself enumerates in — so `Value[0]` is what `Peek`
  answers and a view draws either by walking it.
- **`AtomsMap<TKey, TValue>.TryAdd` and `TryRemove`.** `Add` throws on a key that is taken and
  `Remove` says nothing about whether anything was there; these answer instead, and `TryRemove` hands
  back what it took, which is the lookup and the removal in one step.
- **`foreach` over a list, a map, a queue or a stack** without reaching for `Value`: each carries a
  `GetEnumerator()`. They are still not `IEnumerable<T>` — the enumerator is all a `foreach` asks for,
  and stopping there keeps their own members the only way to change anything. `Value` is still what
  LINQ takes.

## 2.10.0

### Added

- **`AtomsMap<TKey, TValue>`**, the dictionary to `AtomsList<T>`'s list. An atom around a
  `Dictionary<TKey, TValue>` fails the same way an atom around a list does — writing into it reaches
  nobody, and putting the same instance back is taken for a change of nothing — so a map that changes
  entry by entry is held as state of its own:

  ```csharp
  public LocalAtomsMap<string, ServerState> Servers { get; } = new();
  public TrackedAtomsMap<string, string> Overrides { get; } = new(comparer: StringComparer.OrdinalIgnoreCase);

  Servers["build-01"] = ServerState.Online;
  Overrides.Remove("theme");
  ```

  `TrackedAtomsMap<TKey, TValue>` goes on the undo stack, `LocalAtomsMap<TKey, TValue>` does not, and
  each call is one step. Reading one key registers the dependency, so a `Computed<T>` that asks
  `TryGetValue` follows an entry that is not there yet.

  It holds a dictionary but does not implement `IDictionary`: the members it offers are the only way
  in, which is what keeps every change checked against the drawing thread, seen by the frame and
  recorded by the history. It is named a map for the same reason — a type that ends in `Dictionary`
  and is not one is exactly what the naming analyzer objects to.

### Documentation

- **Each package has its own page on NuGet, written for NuGet.** The gallery escapes raw HTML and
  strips relative image paths, so the readme all three packages carried opened with its
  `<p align="center">` spelled out, badges and all, and seventeen broken images below it. The pages
  now live in `nuget/`, one per package: plain Markdown, every image an absolute
  `raw.githubusercontent.com` or `img.shields.io` URL, and the collapsed screenshot gallery —
  `<details>` does not survive either — replaced by a link to the readme on GitHub.
  `Arlecchino.Core` opens on the surface and the atoms rather than on hosting, `Arlecchino.Testing`
  on what a test can reach, and each says which of the three a reader probably wants.

## 2.9.0

### Added

- **`AtomsList<T>.RemoveRange(index, count)`.** A list kept to a length — the last thousand lines of
  output, the newest hundred results — had no way to be trimmed as one change: taking items out one
  at a time notified once per item and came back the same way, and `Reset` copied the whole list on
  every trim, which is the cost the type exists to avoid.

  ```csharp
  if (Lines.Count > Kept)
  {
      Lines.RemoveRange(0, Lines.Count - Kept);
  }
  ```

  `RemoveAt` is now the one-item case of it, so both are a single undo step.

## 2.8.0

### Added

- **A list can be state of its own.** An `Atom<List<T>>` is a trap: adding to the list inside it never
  goes through `Atom.Value`, so nothing is notified, no frame is asked for and the drawing thread is
  not checked — and writing the same instance back does not help either, because an atom compares by
  the default comparer and a list is compared by reference, so the write is taken for a change of
  nothing. `AtomsList<T>` changes in place and still does everything a write does:

  ```csharp
  public LocalAtomsList<string> Log { get; } = new();
  public TrackedAtomsList<Task> Plan { get; } = new();

  Log.Add(line);
  Plan.Add(imported);          // one notification and one undo step for the lot
  Plan.Reset(loaded);
  ```

  The two kinds mirror the two atoms — `TrackedAtomsList<T>` goes on the undo stack and
  `LocalAtomsList<T>` does not — and one call is one step, which is why adding several items at once
  is a member of its own rather than a loop. `Value` is a live, read-only view, so a widget handed it
  once draws whatever is in the list on every later frame.

  The list held in an `Atom<IReadOnlyList<T>>` and replaced wholesale is still the right answer for a
  handful of things that change on a keystroke; the new type is for the ones appended to often or
  long enough that copying hurts. [Atoms](https://the1fest.github.io/Arlecchino.Docs/docs/atoms) says
  which to reach for.

## 2.7.0

### Added

- **Three widgets that draw numbers.** A screen that reports on something had a `ProgressBar` and
  nothing else, so a series over time or a set of things to compare came out as a column of figures
  the reader has to add up themselves:

  ```csharp
  private readonly Sparkline _downloads = new()
  {
      Values = _history,
      Caption = static value => $"{value:0}/s",
  };

  private readonly BarChart<Mirror> _mirrors = new()
  {
      Render = static mirror => mirror.Name,
      Value = static mirror => mirror.Megabytes,
      Items = Mirrors,
      Caption = static value => $"{value:0}",
  };

  private readonly Gauge _disk = new()
  {
      Value = 91,
      Caption = static value => $"{value:0}%",
      Bands = [new(0m, Theme.Active), new(70m, Theme.Warning), new(90m, Theme.Error)],
  };
  ```

  `Sparkline` draws a series as one row of blocks, newest at the right, and shows as much history as
  the row is wide. `BarChart<T>` gives every item a bar measured against the largest of them, with the
  labels in one column and the readouts in another. `Gauge` is a bar against a range that need not
  start at zero, coloured by the bands it crosses, so a fill that has gone past the line says so
  without the view testing the value itself. All three are passive: they draw where they are put and
  hand back the rows below them.

## 2.6.1

### Changed

- **The key screen reads in two columns.** `F1` listed everything in one column, so an application
  with a screenful of its own keys pushed the commands off the bottom and the screen had to be
  scrolled to be read at all. The keys that work everywhere and the keys of the screen it was opened
  from now stand side by side, with the registered commands in a band underneath. A terminal too
  narrow for two columns stacks them as before.

## 2.6.0

### Added

- **A notification can carry more than a line.** Work that takes a while had nowhere to report itself:
  the output row holds one line for a few seconds, and a dialog blocks the screen it is reporting on.
  A notification now takes three optional pieces, so the same entry serves while the work runs and
  after it is over:

  ```csharp
  var entry = state.Notifications.Raise(new(DateTimeOffset.Now, NotificationLevel.Information, "Copying")
  {
      Progress = () => $"{copied} of {total} files",
      Detail = () => string.Join(Environment.NewLine, errors),
      Actions = [new(() => "Stop", cancelling.Cancel)],
  });
  ```

  `Progress` is read every frame and is what the output row and the notifications screen show, so the
  counts climb without the application raising a message per file. `Share` answers how far along the
  work is, and a bar is drawn for it wherever the entry appears — a small one in the row, a full-width
  one in the opened dialog; work whose size is not known answers `null` and gets the text alone.

  An entry that reports progress stays on the output row past `NotificationTimeout`, is never expired
  by `NotificationLifetime`, and survives `Clear()` — a copy does not stop because its line was
  cleared, and a job running with nothing on screen is worse than a list that will not empty.

  `Settle(entry, text, level)` turns that line into what came of the work, in place: the entry keeps
  its spot and its identity, so a dialog someone already has open changes under them instead of going
  stale, its actions are dropped, and it starts ageing from the moment it ended rather than from the
  moment it began. `Withdraw(entry)` still removes one outright.
- **Notifications open.** `Enter` on the notifications screen opens the entry in a dialog that shows
  `Detail` in full — the errors a copy collected, the output of a command — and offers its `Actions`
  as chips, picked with `←→` and run with `Enter`, or clicked. Entries without either simply read as
  the line they carry. The wording is `ArlecchinoStrings.NotificationsOpen` and
  `ArlecchinoStrings.ModalNotificationHints`.

### Fixed

- **A view may ask the container for the `Navigator`.** Until now the navigator showed the start route
  from its own constructor, so a view built for that route could not take a `Navigator` parameter: the
  container was asked for the service it was still building, and the resolve went round that circle
  forever. Nothing was drawn, nothing was logged, and `Ctrl+C` did nothing either, because the terminal
  was taken over further down the same call.

  The start route is now shown the first time the screen is needed — the first frame, key, mouse event
  or `Apply` — rather than while the navigator is being built, so a view that wants to navigate from a
  dialog callback simply asks for it:

  ```csharp
  public sealed class InventoryView : IArlecchinoView
  {
      private readonly Navigator _navigator;

      public InventoryView(Navigator navigator) => _navigator = navigator;
  }
  ```

  `CurrentRoute` still reads as the start route from the moment the application is built, and a screen
  that navigates from its own constructor — which was a hang before — now throws and says what to do
  instead.

## 2.5.0

### Added

- **`ArlecchinoAsyncStore`** is a store that fetches something before it holds the truth — settings
  read from disk, a session restored from a server. Derive from it and the framework starts the load
  as the application starts, with the shutdown token, so the store needs neither a `BackgroundService`
  of its own nor a `TaskCompletionSource` written by hand:

  ```csharp
  public sealed class SettingsStore : ArlecchinoAsyncStore
  {
      public TrackedAtom<string> Server { get; } = new("127.0.0.1");

      protected override async Task LoadAsync(CancellationToken token)
      {
          var saved = await Settings.ReadAsync(token);

          Server.Post(saved.Server);
      }
  }
  ```

  The first frame is drawn without waiting: a terminal that hangs black on a slow disk is worse than a
  screen that says it is loading. Each store answers for itself — `Status`, `Error`, `IsLoading`,
  `IsLoaded` and `Failed` as atoms, so a view that reads them redraws by itself, and `Ready` as a task
  for code outside a view:

  ```csharp
  if (_settings.IsLoading) { ... }        // in a view

  await _settings.Ready;                  // in a worker or a command
  ```

  `Ready` faults with whatever the load threw and is cancelled when the application stopped first, so
  awaiting it says what happened rather than hanging. A store that throws turns its status to failed,
  is logged, and leaves the application running on whatever its atoms already hold.

  `AddGeneratedStores()` and `AddStore<T>()` register such a store for the host to start; a scoped
  store is not started, since it belongs to one screen rather than to the application.

  What is loaded reaches the atoms through `Post`, because `LoadAsync` runs off the drawing thread —
  writing `Value` from there throws and says so.
- **The hints box offers the command palette.** A line for the key that opens it — `: → commands` with
  the default keymap — is added whenever there is at least one command registered, which is the same
  condition under which the key does anything at all. The wording is `ArlecchinoStrings.HintCommands`.

  A view with no hints of its own now gets a box with that one line, where before it had none.

## 2.4.1

### Fixed

- **The file picker opens where a file is, rather than on the drives.** `InitialPath` was checked with
  `Directory.Exists` alone, so a request that named a file — which is what a `Field.Path` for a file
  holds once it has an answer — failed the check and started browsing from nothing. Reopening such a
  field threw the user back to the list of drives every time.

  A folder is browsed as before; a file is browsed in the folder that holds it, and the cursor starts
  on that file, so confirming reopens the same answer. A path that no longer exists still lands on the
  drives.

## 2.4.0

### Added

- **`Field.PathFrom`** says where the picker opens while the field is still empty:

  ```csharp
  Field.PathFrom(() => "Save folder", settings.Folder, ViewKind.Settings, pickFolder: true,
      start: () => state.PickerLastFolder);
  ```

  `Field.Path` opens at the path the field holds, so an empty field left the picker with nowhere to go
  and landed the user on the drives with no way to say otherwise. A field that already has a value
  still opens there and ignores `start`, and a path that no longer exists lands on the drives as
  before. `start` is a delegate rather than a string so "wherever the user was last time" is answered
  when the picker opens rather than when the form is built.

  It is a member of its own rather than another argument to `Field.Path`, because adding a parameter —
  optional or not — changes the signature every application was compiled against. Package validation
  caught exactly that, which is what it is there for.

## 2.3.0

### Added

- **`region.Flow()`** writes a pane line after line without counting rows:

  ```csharp
  var flow = region.Flow();

  flow.AppendLine("PLAYERS", Theme.TableHeader);
  flow.FillLine();

  foreach (var player in players)
  {
      flow.AppendLine(player.Name, Theme.Default);
  }
  ```

  `Surface`'s flow calls belong to the whole frame, so reaching for `region.Surface.AppendLine(...)`
  inside a pane wrote at the top of the screen and painted over the pane's border and its neighbours.
  A `PaneFlow` stays where it was given: everything is written in the region's coordinates, clipped to
  it, and once the pane is full the calls stop doing anything — a loop over more rows than fit needs no
  bound of its own.

  `SkipLine()`, `Skip(rows)`, `FillLine(style)` and `Rewind()` move the cursor; `Rest()` hands back the
  rows it has not reached yet as a region, for giving what is left of a pane to a widget. It is a
  class, so a helper that writes a few more lines carries the cursor along, and two flows over one
  region are independent.

## 2.2.0

### Added

- **`PaneTree`** describes a screen made of panes as one expression instead of a chain of `SplitTop`
  and `SplitLeft` calls spread through `Draw`. Two members build it — `Branch` and `Leaf` — and
  `Draw` puts every pane where the branches say:

  ```csharp
  using static Arlecchino.Layout.PaneSplit;
  using static Arlecchino.Layout.PaneTree;

  _layout = Branch(
      Rows,
      3,
      Leaf(DrawToolbar, () => "toolbar"),
      Branch(
          Rows,
          PaneSize.CellsFromEnd(2),
          Branch(Columns, 0.25, Leaf(_files, () => "files"), Branch(Leaf(_editor), Leaf(_log))),
          Leaf(_status))).Gaps(inner: 1, outer: 1);

  public void Draw() => _layout.Draw(_surface.Content);
  ```

  Only the two halves of a branch are required. Left to itself, a branch halves the space and cuts
  along the longer side — measured in what the eye sees rather than in cells, since a terminal cell is
  about twice as tall as it is wide, and worked out per frame, so it can turn from columns into rows
  when the window is resized. Give it a `PaneSplit` where that matters, which is the chrome.

  Sizes come in the three kinds a terminal screen actually needs: a share of the space (`0.25`), a
  fixed count of cells (`3`), and everything-but-a-count for chrome anchored to the far edge
  (`PaneSize.CellsFromEnd(1)` is a status bar on the last row). `double` and `int` convert on their
  own, so they read as themselves at the call site.

  Spacing belongs to the tree rather than to a call: `Gaps(inner, outer)` names it the way a tiling
  window manager does — between the halves of every branch, and around everything.

  A leaf takes a title — `Leaf(_files, () => "files")` — and is then drawn in a box, with the pane
  handed the room inside it. The border is `Theme.Active` while that widget holds the focus and
  `Theme.Info` while it does not, so a screen of panes shows where the cursor is without the view
  saying anything about it. Titles are `Func<string>` like every other piece of user-visible text.

  `AsFocusRing` builds the focus ring of the screen out of the layout — every pane that takes the
  focus, in the order the branches lay them out — so `Tab` follows the screen instead of a second list
  kept in step by hand:

  ```csharp
  _focus = _layout.AsFocusRing(options.Keymap);
  ```

  Putting one widget instance in two panes throws as the tree is built: a widget remembers the region
  it was drawn into, so the same one twice would draw twice and answer clicks for one of them only.

  Nothing about a frame is kept in the tree — sizes are worked out per `Draw`, so one tree fits every
  terminal and a resize needs no bookkeeping. A region too small for what it holds leaves the panes
  that did not fit empty rather than overlapping them, and drawing into an empty region writes
  nothing. See [Layout](https://the1fest.github.io/Arlecchino.Docs/docs/layout).

  It is a layout description and not a component system: the tree decides where things go and nothing
  else — no lifetimes, no state, no re-render pass — and a view that splits regions by hand keeps
  working as it did.

## 2.1.0

### Added

- **`atom.Post(value)`** writes an atom from a background thread without a lambda around it:
  `_step.Post(value)` where the whole of `FrameThread.Post(() => _step.Value = value)` used to be
  written out. The value is written just before the next frame, in the order it was posted, and
  everything a plain write does — notifying, asking for a repaint, recording an undo step — happens
  then, so a posted edit is undone like any other.

  The `Value` setter still refuses a write from another thread rather than quietly posting it. A write
  that lands later is not a write you can read back, and an atom that hid that would make
  `atom.Value = loaded;` followed by reading `atom.Value` return the old value with nothing to say so.
  `Post` is named for what it does.

  Atoms that have to change together still belong in one `FrameThread.Post` with a block, so that no
  frame falls between them — which is what `AsyncAtom<T>` does internally with its value and its
  status, and what the packages sample does with its log and its line count.

## 2.0.0

The three breaking changes `1.x` announced, delivered together.
[Migrating to 2.0](https://the1fest.github.io/Arlecchino.Docs/docs/migrating-to-2.0) is the edit
list; nothing here needs more than a rename, a delete or a decision about colour.

### Changed

- **`IArlecchinoWidget.Place` is now `Draw`.** The interface has one member, `SurfaceRegion
  Draw(SurfaceRegion region)`: it paints the widget and answers what is left of the region underneath,
  so a view stacks things without counting rows. Rename `Place` to `Draw` at both ends — the
  implementation and every call.
- **The framework's own colours are the default palette.** `new ThemePalette()` is now crimson titles,
  bone text, ash borders and an ink cursor row rather than the terminal's plain sixteen, so an
  application that never called `UseTheme` looks like Arlecchino. `ThemePalette.Basic` is exactly the
  old defaults, and `UseTheme(ThemePalette.Basic)` is the whole of the way back.
  `ThemePalette.Arlecchino` still exists and still means the same thing; it is only redundant now.
- `AsyncAtom<T>` and `ViewLifetime` no longer take a dispatcher: `new AsyncAtom<T>(initial)` and
  `new ViewLifetime()`.

### Removed

- **`UiDispatcher`.** The queue it held moved into `FrameThread`, the type that already knew which
  thread draws, so handing a result back from background work is `FrameThread.Post(...)` with nothing
  injected. `RunPending` and `HasPending` are statics there too. Everything else about it is
  unchanged: posting is safe from any thread, runs in order just before the next frame, asks for that
  frame by itself, and reports an action that threw without dropping the rest.
- **The obsolete `void IArlecchinoWidget.Draw`,** along with the `ARL0001` diagnostic id that existed
  to let its deprecation be silenced on its own. A `#pragma warning disable ARL0001` left behind no
  longer disables anything.

### Added

- `FrameThread.DiscardPending()` drops work that was posted and can no longer run, which is what
  giving up the last claim on the drawing thread does by itself and what `ArlecchinoTestHost` does as
  it is disposed — one test's leftovers never run inside the next.
- `ThemePalette.Basic`, the sixteen plain colours that were the default before this release.

### Fixed

- Posting work while nothing is drawing no longer runs it inline. `FrameThread.Post` ran the action on
  the calling thread when no frame loop had claimed one, so an action that posted itself — the
  ordinary way to say "again next frame" — recursed until the stack ended instead of queueing. It
  always queues now.

## 1.3.0

### Fixed

- **Input no longer runs on a different thread from drawing.** The hosted service runs two loops —
  one reading the terminal, one drawing — and the reader routed what it read there and then. So a key
  press changed the selection, the modal stack, the route and any atom it touched *while* the other
  loop was reading the same things and writing to the `Surface`: no locks, no barriers, and nothing in
  the framework saying so. Instrumenting both loops showed drawing on threads 4, 6, 9 and 10 and input
  on 6 and 9 in a single run — neither loop is pinned, since every `await` resumes wherever the pool
  puts it.

  The reader now queues what it reads and the frame loop drains the queue at the top of each turn,
  before the ticker and before drawing. Everything an application writes is back to being touched by
  one thread, which is what the documentation already claimed. A key press costs at most one frame of
  latency — 16 ms at the default rate.

  This also settles the things that hung off it: `Ticker`'s list, the notification list and every
  widget collection were reachable from both loops and are now reachable from one.
- Giving the terminal back is idempotent, and so is unhooking. Three threads can reach the shutdown
  path — the loop finishing, `ProcessExit`, an unhandled error — and each one used to unsubscribe the
  handlers, walk the list of signal registrations and write its own set of escape sequences over
  whatever the others were writing. It runs once now; the rest walk past. Coming back from `SIGTSTP`
  re-arms it, so the modes are still restored when the application finally exits.
- **Posting work from inside posted work hung the application.** `UiDispatcher.RunPending` drained the
  queue until it was empty, so an action that posted itself — the ordinary way to say "again on the
  next frame" — ran forever inside one frame and the loop never came back. It now runs what was
  waiting when the frame started; anything posted by that work waits for the next one.
- **A resource that registers something while the screen is closing crashed the close.**
  `ViewLifetime.Dispose` disposed its list while iterating it, so a `Dispose` that called `Track`
  threw `Collection was modified` out of navigation. The list is taken as a snapshot now, and
  `Track` after the screen has closed disposes what it is handed instead of holding it forever.
- **The Windows mouse could be started twice or read after being stopped.** `SystemTerminal` keeps the
  console reader in a plain field that the input loop reads on every poll, while `EnableMouse` and
  `DisableMouse` are called from wherever a signal handler happens to run — `SIGTERM` reaches them on
  Windows too. Two calls to `EnableMouse` could leave a second reader running with the console mode it
  set never restored. The field is `volatile` and both transitions are a single interlocked step now:
  a reader that loses the race is stopped rather than orphaned.

### Added

- `ArlecchinoTestHost.DrainInput()` routes what the reader has queued, for a test that drives
  `TerminalInputReader` itself. `ReadFromTerminal` and `Frame` already do it.

## 1.2.0

### Added

- **A palette in the framework's own colours.** `UseTheme(ThemePalette.Arlecchino)` paints the chrome
  in the crimson, bone and ink of the harlequin mask. The background is left to the terminal
  everywhere except the two cursor rows — a selection has to paint behind its text to be one — so it
  reads on a light terminal as readily as on a dark one: it colours the writing, not the screen.
  Crimson is kept for trouble: `Error` is the only filled row wearing it, the cursor row is ash and a
  warning is amber, so a selected row and a failing one are never the same colour.
- **`TermColor` carries exact colours as well as palette ones.** `ExactForeground` and
  `ExactBackground` are drawn where the terminal can do 24-bit, and the `Foreground` and `Background`
  beside them are what everywhere else gets — so a palette can state a brand colour and still degrade
  to the fallback its author picked rather than the nearest one arithmetic found. Existing palettes
  are unaffected: leave the two unset and nothing changes.

## 1.1.0

### Added

- **A widget can now say how much of its region it used.** `IArlecchinoWidget.Place(SurfaceRegion)`
  draws the same thing `Draw` did and returns what is left underneath, so a view stacks one thing
  after another instead of counting rows by hand: `var rest = _header.Place(surface.Content);`. A
  widget that fills whatever it is given — `ListBox`, `Table`, `Tree`, `ScrollPane`, `TextView` —
  answers an empty region; one that owns a known number of rows — `StatusBar`, `ProgressBar`,
  `Spinner`, `Tabs` — answers the rows below it, and `Form` answers whatever the fields did not need.
  The hand-written constant that used to say how tall a header was is what this replaces.

### Deprecated

- **`IArlecchinoWidget.Draw` is deprecated and goes away in 2.0**, where `Place` takes its name. It
  could not simply be changed to return a region: C# does not overload on the return type, so the two
  have to live under different names until the old one is gone.
- Nothing has to change at once. Both members carry a default implementation, so either one on its own
  satisfies the contract: a widget written against 1.0 keeps compiling and its `Place` reports nothing
  left, and a widget written against the new shape still answers a caller that has not migrated. A
  widget that implements *neither* is the one case that does not work — the defaults call each other
  and the frame recurses until the stack ends.
- The warning carries its own diagnostic id, `ARL0001`, rather than plain `CS0618`, so an application
  with `TreatWarningsAsErrors` can silence exactly this deprecation while it migrates instead of
  turning off every obsoletion it has.

## 1.0.1

### Fixed

- **A list no longer takes the application down when its collection shrinks mid-frame.** `ListBox`
  worked out which rows to draw from `Items.Count` and then read them one by one, so anything that
  removed items in between — a background thread that forgot the dispatcher, or a `Render` or
  `ItemStyle` delegate that touches the collection — reached `Items[index]` after the item was gone
  and threw `ArgumentOutOfRangeException` out of `Draw`. The row is skipped now and the frame ends
  early rather than the application ending. `Table` draws through the same list, so it is covered too;
  `Tree` already flattened its nodes into a snapshot first.
- It is not swallowed either: a frame cut short that way is logged once, with the route it happened on
  and a reminder that a widget's collection is changed from the drawing thread — `UiDispatcher.Post`
  when the change comes from anywhere else. A race that used to be a crash is now a warning in the log
  overlay, which is where it belongs.
- The generator's `Microsoft.CodeAnalysis.CSharp` reference went back to the oldest version it
  supports, and Dependabot is told to leave it alone. A bump to `5.6.0` was merged, which sounds
  harmless and is not: a generator runs inside the compiler of the application referencing it, so a
  newer Roslyn reference means it stops loading on an older SDK — `AddGeneratedViews` is missing and
  the user sees `cannot resolve symbol` with nothing to explain it. Released `1.0.0` is unaffected;
  the bump landed after the tag. The bump also turned on `RS2008` and broke the build, which is how it
  was noticed at all.

### Continuous integration

- The branch-coverage floor moved from 70% to 66%, because `coverlet.collector` 10 counts branches
  that 6 did not: with the same 490 tests, line coverage went up (88.0% to 88.7%) while branches fell
  by three to five points in every assembly at once. The measure changed, not what is covered, and the
  floor keeps the same headroom under the new one.
- The consumer application is built twice, on the .NET 10 SDK and on the .NET 8 one. A generator that
  refuses to load in an older compiler is invisible to every other check in the matrix — the
  repository builds, the tests pass, the package is produced — and the only place it shows is an
  application built the way somebody on the long-term support release builds theirs.

## 1.0.0

The first stable release: the public surface is what it is going to be, and from here a breaking
change means a `2.0`. This release is the API review that made that possible, and the features it was
waiting for — everything under **Changed** is breaking, and it is the last release that intends to be.

### Added

- **The packages target `net8.0` as well as `net10.0`.** An application on the long-term support
  release can use them now; the two libraries are built from the same source, which is why `LogBuffer`
  locks on a plain object instead of `System.Threading.Lock`. The suite runs on both frameworks.
- **Work on a clock.** `Ticker` schedules an action `Every(interval)` or `After(delay)`, runs it
  between frames on the drawing thread and asks for a repaint afterwards; the handle it returns
  cancels the work, so `ViewLifetime.Track` ties it to a screen. No thread of its own — the frame loop
  calls it, and `ArlecchinoTestHost.Advance(...)` moves a `TestClock` instead, so a test never waits.
- **Message and confirmation dialogs.** `RequestMessage` shows something to read, wrapped and
  dismissed with either closing key. `RequestConfirmation` asks first with **No** preselected and runs
  the callback only on yes.
- **The output row times out, and keeps a history.** Writing `ArlecchinoState.Output` raises a
  notification: the row shows it for `NotificationTimeout` and then goes quiet, while the message stays
  readable for `NotificationLifetime` on a screen of its own — `Ctrl+N` or a click on the row opens
  `Routes.Notifications`, where `Backspace` clears the list. `UseNotifications(key, timeout, lifetime)`
  configures all of it, `WithoutNotifications()` turns the row off.
- **A keys screen.** `F1` opens `Routes.Help`: every key the framework answers to with what it does,
  then the commands of the screen it was opened from, then the application's commands. The middle
  section is the point — a view's `Commands()` are the keys that work only there, which is what
  somebody pressing `F1` is usually after; a screen with none gets no section rather than an empty
  heading. The descriptions come from `ArlecchinoStrings.HelpKeys` and the heading from
  `HelpScreenSection`, so they translate like everything else.
- **`ScrollPane`**, a window onto content taller than its space, and **`Surface.Clip`** underneath it:
  a scope that confines every write to a rectangle whatever coordinates the caller uses, so content
  drawn at an offset cannot land on a neighbour.
- **`TextView`** for reading a block of text — wrapped, scrolled, reflowed when the width changes —
  and **`TextWidth.Wrap`** behind it, public for layout code of your own.
- **`TextAreaModal` and `RequestTextArea`** for editing several lines: `Enter` breaks the line, the new
  `Submit` binding (`Ctrl+Enter`) confirms, the caret moves by symbols across line ends, pasted blocks
  keep their breaks, and the validator's message is drawn under the text. `Copy` takes the whole text
  to the clipboard.
- **The notification list is bounded.** `Notifications.Capacity` (200) caps it however young the
  messages are, so reporting in a loop no longer grows it without limit.
- **A binding can carry two combinations.** `KeyBinding` gained `AlsoKey` and `AlsoModifiers`, so
  `Copy` answers to both `Ctrl+Insert` and `Ctrl+Shift+C` — the two habits for the same action. Pasting
  needs nothing here: the terminal turns `Ctrl+Shift+V` into a bracketed paste, which already arrives
  as one block.
- **`ArlecchinoReport`**, for when a user says it looks wrong on their machine. `Describe()` returns
  the version, the runtime and platform, what the terminal said it can do (`TERM`, `COLORTERM`,
  `NO_COLOR`, size, colour level, whether output is redirected), the route being shown with the modals
  above it, and the options the application was built with. It carries no field values and nothing the
  user typed, so it can go straight into a public issue — which is what the issue template now asks
  for. A command that copies it to the clipboard is three lines.
- `AddStore<T>()`, so a store can be registered by hand as views, commands and widgets already could —
  scoped when the type implements `IArlecchinoScopedStore`, singleton otherwise.

### Changed

- **`Arlecchino.State` is laid out by subject.** It split three ways: atoms and stores to
  `Arlecchino.Atoms`, every modal to `Arlecchino.Modals`, and `ArlecchinoState` with the file-picker
  request left where they were. `TerminalInputReader` moved to `Arlecchino.Input`.
- **`TuiState` is `ArlecchinoState`** — the last name carrying the old prefix.
- **The atom vocabulary is finished.** `IReadableState<T>` is `IReadableAtom<T>`, `StateHistory` is
  `AtomHistory`, `AsyncState<T>` is `AsyncAtom<T>`, and `IStateEdit` is `IAtomEdit`.
- **Every contract an application implements carries the package name**: `IViewFactory`,
  `ITerminal`, `IFocusable` and `ITermColor` are now `IArlecchinoViewFactory`,
  `IArlecchinoTerminal`, `IArlecchinoFocusable` and `IArlecchinoColor`. Interfaces only the framework
  implements — `IReadableAtom<T>`, `IAffixedModal` and the rest — deliberately keep the short name.
- **Diagnostics are `ARL001`–`ARL007`**, not `TSR*`.
- **`Style` means one thing.** The per-item delegate on `ListBox<T>`, `Table<T>` and `Tree<T>` is
  `ItemStyle`; `Style` stays the single colour on `ProgressBar`, `StatusBar` and `Spinner`.
- **`Form` has one input surface.** `Handle` and `HandleMouse` return a `FocusResult` like every other
  widget, instead of a `ViewRoute` from the public method and a `FocusResult` from an explicit
  implementation. A view hosting a form returns `_form.Handle(key).Route`.

### Fixed

- **Undo groups nest.** `AtomHistory.Group()` counted nothing, so a group opened inside another closed
  the whole thing when it was disposed, and every edit after it became a second undo step. Wrapping
  code that groups edits of its own quietly lost the atomicity the outer group asked for. Groups are
  counted now: one step, undone in one go.
- **A screen that cannot be built no longer moves the application.** The navigator changed the current
  route and disposed the screen it was leaving *before* the new one was constructed, so a view whose
  constructor threw — a store that was never registered is the usual cause — left the route pointing
  at a screen that does not exist while the old one carried on drawing. `Back()` and the diagnostics
  disagreed with the screen from then on. The new screen is now built first: if it throws, the route,
  the history and the screen are exactly as they were, and the error reaches the log and the output
  row as before.
- A view, store, command or widget the generated code cannot name is left out of it. A view nested
  privately inside another type was picked up and registered, and the build then failed with `CS0122`
  in a generated file. Reachability is checked through every containing type now.
- A view, store, command or widget declared inside another type is now named through it. The generator
  emitted `new ModsView(...)` for a class nested in `Screens`, which does not compile — the code it
  wrote could not see the type it had just found. All four generators name types the same way now.
- A view, store or command without a public constructor is now left out of the generated code instead
  of being registered anyway. The generator reported it (`ARL002`, `ARL005`, `ARL006`) and then emitted
  a `new` of it regardless, so the diagnostic arrived alongside a compiler error in generated code
  rather than in place of one. Widgets already behaved this way; the other three now match.

### Removed

- Implementation details are no longer public: `ArlecchinoHostedService`, `RegisteredViewFactory`,
  `ViewRegistrations`, `CommandConflicts`, `LogOverlay`, `ArlecchinoLoggerProvider`, `FilePickerView`,
  `EscapeSequenceParser`, `AtomChanges` and `AtomTracking`. The constructors of `Navigator`, `Screen`
  and `InputRouter` went with them — the container builds those, an application resolves them.

### Documentation

- The packages carry an icon, and the README opens with the banner. The brand assets live in
  `assets/`: the harlequin mask as an icon on its plate, transparent, and as a single-colour glyph
  that inherits `currentColor`, plus the banner and the social card — SVG throughout, with the raster
  sizes rendered beside them.
- [Rendering](https://the1fest.github.io/Arlecchino.Docs/docs/rendering) ends with the terminals the
  framework has actually run in and what each one showed — plain `xterm-256color`,
  `COLORTERM=truecolor`, `NO_COLOR`, `TERM=dumb`, tmux, macOS on Arm — and, just as usefully, the
  ones it has not: conhost without virtual terminal support, Terminal.app, PuTTY, kitty and friends.
- `Theme.Palette` and `TerminalCapabilities.Color` are documented as process-wide, which is what they
  have always been: one look per process, last host built wins, and a test that changes either shares
  the change with everything else running.

### Performance

- **Reading and writing an atom no longer allocates.** A read used to hand `AtomTracking` a delegate
  so that an enclosing `Computed` could discover the dependency, and built one whether or not anything
  was collecting — 64 bytes on every read, on a path frames take constantly. The read now asks first.
  Subscribers are held in an array replaced on subscribe instead of a list copied on every write, so a
  write no longer allocates either; a listener that unsubscribes while being notified still runs to
  completion, and one that subscribes there hears the next write rather than the current one. Reading
  a cached `Computed` went from 3.0 ns and 64 B to 0.5 ns and nothing, writing an atom twenty things
  listen to from 18 ns and 184 B to 8.3 ns and nothing, and re-running a `Computed` allocates 304 B
  rather than 480. The benchmarks that found this are in the repository.

### Tests

- `SystemTerminal` is covered. Until now every test went through `FakeTerminal`, so the escape
  sequences an application actually sends — the alternate screen, bracketed paste, SGR mouse
  reporting, `OSC 52` copying — were never executed by anything. The suite now asserts the bytes, and
  the platform split with them: away from Windows the mouse is asked for with sequences, on Windows
  nothing reaches the output because the console is read record by record instead.
- Translating a Windows console record into a key or a mouse event moved out of the P/Invoke wrapper
  into a type of its own, which the suite drives directly on either platform: presses, releases,
  drags, a wheel in both directions, held buttons that must not report twice, and the modifiers each
  event carries.
- **Localization is enforced, not trusted.** One test replaces every delegate on `ArlecchinoStrings`
  by reflection and fails if a word of the framework's English survives on the main screen, the keys
  screen, the notification list or a modal — a hardcoded literal is now a failing test rather than a
  bug report from somebody translating the chrome.
- **The documentation is checked against the code.** Every translatable string, every key binding and
  every generator diagnostic has to appear on its page, so the tables stop drifting behind the type
  they describe; the test names what is missing.
- Three sets of tests for what an application does at the edges rather than in the middle: empty and
  zero-sized input (a list with nothing in it, a pane whose content is empty, wrapping to no width, a
  ticker asked to run every no time), robustness (a click outside the frame, a 200 000-character
  paste, an async atom loaded twice and cancelled mid-flight, a validator that refuses everything),
  and boundaries (closing a modal when none is open, writing outside the surface, a form with no
  fields, two commands claiming one key, undo with nothing to undo). Thirty-one cases, one real bug —
  the nested undo groups above.
- Nothing piles up as screens come and go. A hundred visits to a screen that subscribes to an atom
  through `ViewLifetime.Track` leave exactly one subscriber behind, a scoped store is created and
  disposed once per visit, and work scheduled on the ticker stops when the screen does — the three
  ways a long-running terminal application usually starts leaking.
- Resizing is tested through the widgets rather than only through the diff: a list keeps its selection
  on screen when the window shrinks, a scrolled pane comes back into range, text reflows when the
  window narrows, nothing is drawn wider than the window, and the too-small notice appears and goes
  away as the size crosses the minimum.
- The file picker's `Places` are tested. Shortcuts an application puts in the sidebar had no test at
  all: that they are listed, that they come before the folders the framework offers, that one without
  an icon gets the default, and that clicking one browses to it.
- Benchmarks cover what the earlier ones left out: a key through the router, a click, a pasted block,
  writing atoms watched and unwatched, a computed value read cached and invalidated, undo and redo,
  and `TextWidth.Wrap`. They are what found the allocation above.

### Packaging

- **The package is checked against the last release.** `EnablePackageValidation` runs APICompat during
  `dotnet pack`: the `net8.0` and `net10.0` surfaces have to match each other, and from `1.0.1` on they
  are compared with `1.0.0` as well. The baseline is conditional on the version, so it starts applying
  by itself after this release, and a missing baseline fails the pack rather than passing quietly.
- Each package carries release notes pointing at its own section of the changelog.

### Continuous integration

- The build fails on a ReSharper inspection as well as on a compiler warning: `jb inspectcode` runs
  against `.editorconfig` and annotates what it finds. That covers the rules the compiler has no say
  in — a redundant type in an argument, an `if` worth inverting, a member that should be static.
- CI builds a console application against the freshly packed `.nupkg` files, with views, a store, a
  widget and a command in it, registered both by the generator and by hand. Three bugs this cycle only
  showed up that way, and none of them were visible from a build of the repository itself. The project
  is generated outside the checkout, so it is shaped by the packages rather than by our build props.
- The AOT claim is tested rather than asserted. `IsAotCompatible` only turns on an analyzer, so CI now
  publishes the sample with `PublishAot`, runs the native binary and fails unless it draws a frame —
  the failure mode being an application that compiles clean, publishes clean and then shows an empty
  screen because the trimmer took a registration with it. The probe is `-p:AotProbe=true` on the
  sample; the binary is about 5 MB and needs no runtime installed.
- Coverage is measured on every run and the build fails when it drops: 85% of lines, 70% of branches.
  The figures per assembly land in the run summary, so a change that adds code without tests is
  visible before it is merged rather than after.
- CodeQL analyses the C# on every push and once a week, and Dependabot proposes dependency and action
  updates monthly, grouped so they arrive as a few pull requests rather than many.
- Every benchmark is executed on each run as a dry job. Measurements from a shared runner mean
  nothing, but a benchmark that no longer compiles or has started throwing is caught the day it
  breaks; `benchmarks.yml` runs them properly on demand and writes the tables into the run summary.

## 0.9.0

### Fixed

- A form left a blank row under the selected field even when that field had no help, so moving
  through fields dragged a hole along with the selection. The help line is drawn only when there is
  help to show.

### Changed

- **Breaking.** `Form.ReserveHelpRow` is gone with it: keeping a row free for help that does not
  exist was the hole itself, and there is nothing left to configure.

## 0.8.0

### Added

- Widgets of the application can come from the container. A fourth generator finds every
  `IArlecchinoWidget` declared in the project and `.AddGeneratedWidgets()` registers each as a
  **singleton**, built by a factory calling its public constructor with the most parameters — so one
  instance is shared by every screen resolving it, state and focus included. Only the project's own
  widgets are registered; the built-in ones live in the package's assembly and are still constructed
  in the view.
- `.AddWidget<T>()` registers one widget by hand, for a widget the generator cannot see. As with
  commands, it is an alternative to the generated call rather than a layer on top.
- `TSR007` names a widget the container cannot build — generic, no public constructor, or `required`
  members — instead of emitting code that would not compile. `ArlecchinoGenerateWidgets` turns the
  generator off.
- `ArlecchinoKeymap` and `ArlecchinoStrings` are resolvable services now, so a widget or store built
  by the container takes the keymap directly instead of reaching through `ArlecchinoOptions`.

### Fixed

- `LogBuffer` could fall below its own capacity: the check for a full buffer and the removal of the
  oldest line were separate steps, so two threads logging at once dropped the same surplus line
  twice. Trimming happens under a lock now.

## 0.7.0

### Added

- Widgets have a contract. `IArlecchinoWidget` is `Draw(SurfaceRegion)` — what a reusable piece of a
  screen does — and `IArlecchinoInteractiveWidget` adds the input half through `IFocusable`, which is
  what a `FocusRing` cycles. Everything built in answers one of the two, and a widget of your own
  implements the same interface rather than following a convention.

### Changed

- **Breaking.** `ProgressBar`, `StatusBar` and `Spinner` take their colour as a `Style` property
  instead of an argument to `Draw`, so every widget is drawn by the same call. `Spinner.Draw` paints
  the top-left cell of the region it is given rather than taking a row and a column — pass it the
  cell, `region.SplitLeft(region.Width - 1).Right` and friends.

## 0.6.0

### Added

- Commands register themselves. A third generator finds every `IArlecchinoCommand` in the project and
  `.AddGeneratedCommands()` puts each one in the container as a singleton, built by a factory calling
  its public constructor with the most parameters. `TSR006` reports a command the container cannot
  build, and `ArlecchinoGenerateCommands` turns the generator off. `AddCommand<T>()` stays for
  commands that come from another assembly — the two are alternatives, and using both for the same
  type lists it twice in the palette.

### Changed

- **Breaking.** The markers an application implements carry the package name now: `IView` is
  `IArlecchinoView`, `IStore` is `IArlecchinoStore`, and `IScopedStore` is
  `IArlecchinoScopedStore`. All three sit in namespaces an application imports, where a bare `IView`
  or `IStore` is the same name half the ecosystem uses. `IViewFactory` and the rest of the
  navigation types are unchanged — nothing outside the package implements them.
- **Breaking.** `Rendering.FontStyle` is now `TextStyle` and `Rendering.Region` is now
  `SurfaceRegion`. Those two were the whole measured overlap of the public surface with anything
  outside it: both collide with `System.Drawing` at the same arity, so a project that also
  references `System.Drawing.Common` could not import `Arlecchino.Rendering` without qualifying
  them. Nothing else among the 112 public types clashes with the .NET reference assemblies.
- **Breaking.** The atoms are called atoms in the API, not only in the prose: `State<T>` is
  `Atom<T>`, `TrackedState<T>` is `TrackedAtom<T>`, and `LocalState<T>` is `LocalAtom<T>`. The base
  type also stops carrying the name of the namespace it lives in. `AsyncState<T>`, `Computed<T>`,
  `StateHistory` and `StateChanges` keep their names — they are not atoms.

## 0.5.0

### Added

- Stores register themselves. A class of atoms marked `IStore` is found by a second generator, and
  `.AddGeneratedStores()` puts every one of them in the container as a singleton — built by a factory
  calling its public constructor with the most parameters, so nothing is resolved by reflection.
  `IScopedStore` registers as scoped instead, living exactly as long as the screen that asked for it.
  `TSR005` reports a store the container cannot build, and `ArlecchinoGenerateStores` turns the whole
  thing off.

### Documentation

- The two atom types are named where an atom is described rather than only in the state chapter: the
  XML documentation of `Field` and `Form`, the page tables, and the opening of
  [Atoms](https://the1fest.github.io/Arlecchino.Docs/docs/atoms).

### Continuous integration

- `build.yml` ignores `**.md`, `docs/**` and `LICENSE`, and the documentation says how to keep a
  work-in-progress commit off CI entirely (`[skip ci]`, which Actions reads by itself).

## 0.4.0

### Changed

- Whether an atom is undoable is now the type it is created as, not a flag set on it afterwards.
  `State<T>` is abstract; `TrackedState<T>` records its edits on the undo stack and `LocalState<T>`
  never does. Everything that takes an atom still takes `State<T>`, so call sites are unchanged —
  `new State<int>(0) { RecordsHistory = false }` becomes `new LocalState<int>(0)`, and the rest
  becomes `new TrackedState<T>(…)`.
- `State<T>.SetWithoutHistory` is gone with it: the type of the atom is the whole answer, and undo
  restores values through its own path.

## 0.3.0

### Fixed

- The generated view factory named each view by its short name, so a view in a namespace the generated
  file did not sit under failed to compile with `CS0246`. Namespaces of the views are emitted as
  `using` directives now, and views may live anywhere in the project.
- A project with no view yet had nothing generated at all, so `AddGeneratedViews` and `ViewKind` did
  not exist and the error was `cannot resolve symbol` on the first line of the setup. Both are emitted
  from the moment the package is referenced — `ViewKind` simply holds no routes — and the new `TSR004`
  says why.

### Documentation

- The `using` for the generated namespace (`$(RootNamespace).Navigation` by default) is in the README
  and the getting-started example; it was the one line a new application could not guess.
- `IView` is documented with `HandlePaste` and `Commands`, the options table with `BracketedPaste` and
  `EscapeTimeout`, the strings table with `ListPosition`, the form and the log-overlay text, and the
  assembly table with the `Focus`, `Forms`, `Widgets` and `Diagnostics` namespaces and the
  `Arlecchino.Testing` package.

## 0.2.0

First release published on NuGet.

### Added

- Text fields are edited as a real line: a caret drawn where the next character goes, `←`/`→`,
  `Ctrl+←`/`Ctrl+→` by word, `Home`/`End`, `Delete`, `Ctrl+Backspace` and `Ctrl+U`. The logic lives in
  `TextEditing` and applies to the number field too.
- Modals stack. `TuiState.PushModal` opens one over another so a callback can ask a follow-up
  question; closing it uncovers the one underneath, and every level is drawn, offset.
- Bracketed paste. Pasted text arrives as one block through `IView.HandlePaste` or straight into the
  open field, instead of a burst of key presses. On by default (`options.BracketedPaste`).
- Copying a field to the clipboard with `Ctrl+Insert`, encoded as OSC 52 so it works over SSH.
- Scroll bars and a `3/40` position readout in lists, tables, trees and choice modals that hold more
  than fits. `ScrollBar` is public for panes laid out by hand.
- Mouse support on Windows, read from the console's own event queue with `ReadConsoleInput` — the
  platform cannot deliver SGR reports without silencing the keyboard. Quick-edit selection is turned
  off while it runs and restored afterwards.
- A log overlay on `Ctrl+L`. `ArlecchinoLoggerProvider` keeps the last lines in memory rather than
  painting them over the frame, and the overlay scrolls back through them.
- XML documentation on the whole public API of all three packages, enforced by `CS1591` with warnings
  as errors.
- A second sample, `Arlecchino.Processes`: the process list in a sortable table, read in the background,
  filtered from a modal, with a details screen.
- Benchmarks under `benchmarks/Arlecchino.Benchmarks` for frame composition and text measurement.
- The public API of all three packages is now written down in `PublicAPI.*.txt` and enforced by
  `Microsoft.CodeAnalysis.PublicApiAnalyzers`, so a change to the surface cannot land unnoticed.

### Fixed

- `LogBuffer` held its lines in a plain list while logging arrives from any thread; it is a concurrent
  queue now and the overlay draws from a snapshot.
- Editing worked in `char` values, so backspace could cut an emoji or a combining sequence in half.
  Movement and deletion go by symbols, and `TextWidth` exposes the boundary helpers.
- A value longer than the terminal hid the caret; the field scrolls now, with `…` on the side that
  continues.
- An escape sequence split across two reads — normal over ssh — was delivered as `Esc`, `[`, `A`. The
  reader waits `options.EscapeTimeout` for the rest.
- The undo stack grew for the lifetime of the process; it is bounded by `StateHistory.Capacity`.
- Being killed (`SIGTERM`, `SIGHUP`) or suspended (`Ctrl+Z`) left the terminal in the alternate screen
  with no cursor. Both are handled now, and `SIGCONT` restores the modes and repaints.
- `ArlecchinoTestHost` drew whatever colour the machine running the tests happened to allow, so a build
  agent with `NO_COLOR` set produced frames with no styling in them and assertions on colour failed. It
  fixes `TerminalCapabilities.Color` at `TrueColor` instead.

### Changed

- Each screen is built in its own DI scope, so views can take scoped services. `IViewFactory.TryCreate`
  now receives the `IServiceProvider` to build from, and `ViewResolver.Create` returns an `ActiveView`
  that owns the scope.
- `ViewLifetime` ties background work to the screen: `Loading<T>()` cancels with it, `Track` disposes
  with it, `Closing` is the token for work started by hand.
- `AsyncState.Cancel()` drops the status back to `Idle` instead of leaving it `Loading`, so a spinner
  stops when the load is abandoned.

- A validation message now follows the field as it is edited and clears the moment the input becomes
  valid, instead of disappearing on the next keystroke. Nothing is reported before the first attempt
  to submit.
- `ITerminal` gained `MouseAvailable`, `ReadMouse`, `EnablePaste`, `DisablePaste` and
  `CopyToClipboard`. Custom terminals have to implement them.
- `TuiState.CloseModal` closes the top modal rather than all of them; `CloseAllModals` does what it
  used to.

## 0.1.0

First release: cell-grid renderer with diff output, view navigation and history, the modal set, forms
bound to atomic state, focus, the widget set, the command palette and per-view commands, the source
generator, mouse support, theming, localisation through delegates, and the headless test host.
