using System;
using System.Collections.Generic;
using System.Globalization;
using Arlecchino.Input;

namespace Arlecchino.Hosting;

/// <summary>
/// Every piece of text the framework itself draws. All of it is delegates with English defaults, so
/// an application points them at its own resolver and switches language process-wide without the
/// framework knowing that languages exist. They are called on the frames that need them, so nothing
/// has to be rebuilt when the language changes.
/// </summary>
public sealed class ArlecchinoStrings
{
    /// <summary>Footer of a text field.</summary>
    public Func<string> ModalTextHints { get; set; } = static () => "Enter — confirm   Esc — cancel";

    /// <summary>Footer of the command palette.</summary>
    public Func<string> ModalCommandHints { get; set; } = static () => "press a key   Esc — cancel";

    /// <summary>Footer of a single-choice list.</summary>
    public Func<string> ModalChoiceHints { get; set; } = static () => "↑↓ — move   Enter — pick   Esc — cancel";

    /// <summary>Footer of a multi-choice list.</summary>
    public Func<string> ModalMultiChoiceHints { get; set; } =
        static () => "↑↓ — move   Space — mark   Enter — confirm   Esc — cancel";

    /// <summary>Footer of a number field.</summary>
    public Func<string> ModalNumberHints { get; set; } =
        static () => "↑↓ — step   PgUp/PgDn — jump   Enter — confirm   Esc — cancel";

    /// <summary>Footer of a slider.</summary>
    public Func<string> ModalSliderHints { get; set; } =
        static () => "←→ — adjust   Home/End — ends   Enter — confirm   Esc — cancel";

    /// <summary>Footer of a yes/no dialog.</summary>
    public Func<string> ModalToggleHints { get; set; } = static () => "←→ — switch   Enter — confirm   Esc — cancel";

    /// <summary>Title of the screen listing every key.</summary>
    public Func<string> HelpTitle { get; set; } = static () => "Keys";

    /// <summary>Heading over the keys the framework itself answers to.</summary>
    public Func<string> HelpFrameworkSection { get; set; } = static () => "Everywhere";

    /// <summary>Heading over the commands of the screen the help was opened from, given its route.</summary>
    public Func<string, string> HelpScreenSection { get; set; } = static route => $"On {route}";

    /// <summary>Heading over the application's own commands.</summary>
    public Func<string> HelpCommandsSection { get; set; } = static () => "Commands";

    /// <summary>Shown in place of the command list when the application registered none.</summary>
    public Func<string> HelpNoCommands { get; set; } = static () => "no commands registered";

    /// <summary>Line under the title, saying how to leave.</summary>
    public Func<string> HelpClose { get; set; } = static () => "back";

    /// <summary>What each key the framework answers to does, in the order the screen lists them.</summary>
    public Func<ArlecchinoKeymap, IReadOnlyList<(KeyBinding Key, string Action)>> HelpKeys { get; set; } =
        static keymap =>
        [
            (keymap.Back, "go back"),
            (keymap.Forward, "go forward"),
            (keymap.Confirm, "confirm, open, activate"),
            (keymap.Cancel, "cancel, close, leave"),
            (keymap.NextField, "next pane or field"),
            (keymap.PreviousField, "previous pane or field"),
            (keymap.MoveUp, "move up"),
            (keymap.MoveDown, "move down"),
            (keymap.MoveLeft, "move left, collapse"),
            (keymap.MoveRight, "move right, expand"),
            (keymap.JumpUp, "jump up a page"),
            (keymap.JumpDown, "jump down a page"),
            (keymap.First, "go to the start"),
            (keymap.Last, "go to the end"),
            (keymap.Mark, "mark a row, flip a toggle"),
            (keymap.Erase, "delete backwards, clear"),
            (keymap.DeleteForward, "delete forwards"),
            (keymap.EraseWord, "delete the word before the caret"),
            (keymap.EraseToStart, "delete to the start of the line"),
            (keymap.WordLeft, "caret a word left"),
            (keymap.WordRight, "caret a word right"),
            (keymap.Copy, "copy the field being edited"),
            (keymap.PickCurrentFolder, "pick the folder that is open"),
            (keymap.ToggleLog, "show or hide the log"),
            (keymap.Notifications, "open the notifications"),
            (keymap.Help, "open this screen"),
        ];

    /// <summary>Title of the notifications screen.</summary>
    public Func<string> NotificationsTitle { get; set; } = static () => "Notifications";

    /// <summary>Line under that title, saying how many are held.</summary>
    public Func<int, string> NotificationsCount { get; set; } =
        static count => count == 1 ? "1 message" : $"{count} messages";

    /// <summary>Shown when nothing has been said lately.</summary>
    public Func<string> NotificationsEmpty { get; set; } = static () => "nothing to report";

    /// <summary>Hint for reading one notification in full.</summary>
    public Func<string> NotificationsOpen { get; set; } = static () => "open";

    /// <summary>Hint for throwing the list away.</summary>
    public Func<string> NotificationsClear { get; set; } = static () => "clear";

    /// <summary>Hint for leaving the screen.</summary>
    public Func<string> NotificationsClose { get; set; } = static () => "back";

    /// <summary>The key line under the multi-line text dialog.</summary>
    public Func<string> ModalTextAreaHints { get; set; } =
        static () => "Enter — new line   Ctrl+Enter — confirm   Esc — cancel";

    /// <summary>The key line under a dialog that only has something to say.</summary>
    public Func<string> ModalMessageHints { get; set; } = static () => "Enter — close   Esc — close";

    /// <summary>The key line under an opened notification that has something to do about it.</summary>
    public Func<string> ModalNotificationHints { get; set; } =
        static () => "←→ — pick   Enter — do it   Esc — close";

    /// <summary>The affirmative chip of a yes/no dialog.</summary>
    public Func<string> Yes { get; set; } = static () => "Yes";

    /// <summary>The negative chip of a yes/no dialog.</summary>
    public Func<string> No { get; set; } = static () => "No";

    /// <summary>Footer of a date field.</summary>
    public Func<string> ModalDateHints { get; set; } =
        static () => "←→ — field   ↑↓ — change   digits — type   Enter — confirm   Esc — cancel";

    /// <summary>Footer of a time field.</summary>
    public Func<string> ModalTimeHints { get; set; } =
        static () => "←→ — field   ↑↓ — change   digits — type   Enter — confirm   Esc — cancel";

    /// <summary>Footer of the color picker.</summary>
    public Func<string> ModalColorHints { get; set; } =
        static () => "↑↓ — channel   ←→ — adjust   Enter — pick   Esc — cancel";

    /// <summary>Label of the hue channel.</summary>
    public Func<string> ColorHue { get; set; } = static () => "Hue";

    /// <summary>Label of the saturation channel.</summary>
    public Func<string> ColorSaturation { get; set; } = static () => "Saturation";

    /// <summary>Label of the lightness channel.</summary>
    public Func<string> ColorLightness { get; set; } = static () => "Lightness";

    /// <summary>Shown when a number field holds something that will not parse.</summary>
    public Func<string> NotANumber { get; set; } = static () => "must be a number";

    /// <summary>Shown when an email field holds something that is not an address.</summary>
    public Func<string> NotAnEmail { get; set; } = static () => "must be an email address";

    /// <summary>Shown when a link field holds something that is not an http address.</summary>
    public Func<string> NotAUrl { get; set; } = static () => "must be a http or https link";

    /// <summary>
    /// Shown when a number is outside its bounds. The values arrive already formatted, affixes
    /// included.
    /// </summary>
    public Func<string, string, string> OutOfRange { get; set; } =
        static (minimum, maximum) => $"must be between {minimum} and {maximum}";

    /// <summary>How many options are marked, shown in the title of a multi-choice list.</summary>
    public Func<int, string> SelectedCount { get; set; } =
        static count => count == 1 ? "1 selected" : $"{count} selected";

    /// <summary>The filter line above a list, with whatever has been typed so far.</summary>
    public Func<string, string> Filter { get; set; } = static filter => $"Filter: {filter}";

    /// <summary>Shown in place of a list when the filter matches nothing.</summary>
    public Func<string> NothingMatches { get; set; } = static () => "nothing matches";

    /// <summary>Title of the log overlay, with how many lines are held.</summary>
    public Func<int, string> LogTitle { get; set; } = static count => $"Log ({count})";

    /// <summary>Shown in the log overlay while nothing has been logged.</summary>
    public Func<string> LogEmpty { get; set; } = static () => "nothing logged yet";

    /// <summary>The key line under the log overlay.</summary>
    public Func<string> LogHints { get; set; } =
        static () => "↑↓ scroll · End latest · Backspace clear · Esc close";

    /// <summary>
    /// Where the cursor is in a list that does not fit on screen. The position is one-based, since it
    /// is read by a person rather than an index.
    /// </summary>
    public Func<int, int, string> ListPosition { get; set; } =
        static (position, count) => $"{position}/{count}";

    /// <summary>Stands in for a value that has not been set, in forms and fields.</summary>
    public Func<string> Empty { get; set; } = static () => "empty";

    /// <summary>Hint for moving between form fields.</summary>
    public Func<string> FormMove { get; set; } = static () => "move";

    /// <summary>Hint for opening the field under the cursor.</summary>
    public Func<string> FormEdit { get; set; } = static () => "edit";

    /// <summary>Hint for clearing the field under the cursor.</summary>
    public Func<string> FormReset { get; set; } = static () => "reset";

    /// <summary>Title of the hints box.</summary>
    public Func<string> KeysTitle { get; set; } = static () => "Keys";

    /// <summary>
    /// What the hints box calls the command palette. The framework adds the line itself, beside the
    /// key that opens it, whenever there is at least one command to show.
    /// </summary>
    public Func<string> HintCommands { get; set; } = static () => "commands";

    /// <summary>Title of the command palette.</summary>
    public Func<string> CommandPaletteTitle { get; set; } = static () => "Commands";

    /// <summary>Shown when a key pressed in the palette belongs to no command.</summary>
    public Func<string, string> CommandUnknown { get; set; } = static key => $"unknown command: {key}";

    /// <summary>
    /// Shown on the output line when a view or a callback throws. The application keeps running, so
    /// this is what the user sees instead of a crash.
    /// </summary>
    public Func<string, string> ViewFailed { get; set; } = static message => $"error: {message}";

    /// <summary>Headline of the notice that replaces the view in a too-small window.</summary>
    public Func<string> TerminalTooSmall { get; set; } = static () => "Terminal window is too small";

    /// <summary>Formats a window size, used for both the current and the required one.</summary>
    public Func<int, int, string> TerminalSize { get; set; } = static (width, height) => $"{width} x {height}";

    /// <summary>Introduces the required size in that notice.</summary>
    public Func<string> TerminalNeeded { get; set; } = static () => "needed at least";

    /// <summary>Text of the file picker, which has enough of its own to be grouped separately.</summary>
    public FilePickerStrings FilePicker { get; set; } = new();

    /// <summary>Text of the file picker: labels, column headers, and the three formatters.</summary>
    public sealed class FilePickerStrings
    {
        /// <summary>Title used when a request does not carry one of its own.</summary>
        public Func<string> Title { get; set; } = static () => "Pick a path";

        /// <summary>Shown beside the title when the picker is choosing a folder.</summary>
        public Func<string> FolderMode { get; set; } = static () => "folder";

        /// <summary>Shown beside the title when the picker is choosing a file.</summary>
        public Func<string> FileMode { get; set; } = static () => "file";

        /// <summary>The drive list, shown as a place and as the location above every drive.</summary>
        public Func<string> Drives { get; set; } = static () => "This computer";

        /// <summary>Heading above the user folders in the sidebar.</summary>
        public Func<string> Favorites { get; set; } = static () => "Favorites";

        /// <summary>Heading above the home folder and the drives.</summary>
        public Func<string> Locations { get; set; } = static () => "Locations";

        /// <summary>Label of the filter field in the toolbar.</summary>
        public Func<string> Search { get; set; } = static () => "Search";

        /// <summary>Header of the name column.</summary>
        public Func<string> ColumnName { get; set; } = static () => "Name";

        /// <summary>Header of the modification date column.</summary>
        public Func<string> ColumnDateModified { get; set; } = static () => "Date Modified";

        /// <summary>Header of the size column.</summary>
        public Func<string> ColumnSize { get; set; } = static () => "Size";

        /// <summary>Header of the kind column.</summary>
        public Func<string> ColumnKind { get; set; } = static () => "Kind";

        /// <summary>How many entries the current folder shows, on the status row.</summary>
        public Func<int, string> ItemCount { get; set; } = static count => count == 1 ? "1 item" : $"{count} items";

        /// <summary>Kind shown for a directory.</summary>
        public Func<string> KindFolder { get; set; } = static () => "Folder";

        /// <summary>Kind shown for a drive.</summary>
        public Func<string> KindVolume { get; set; } = static () => "Volume";

        /// <summary>
        /// Turns a file extension into a readable kind. The default knows the common ones and falls
        /// back to <c>XYZ file</c>.
        /// </summary>
        public Func<string, string> KindOf { get; set; } = DefaultKind;

        /// <summary>
        /// Formats a modification date. The default says <c>Today at 9:41</c> and
        /// <c>Yesterday at 9:41</c> before falling back to a full date.
        /// </summary>
        public Func<DateTime, string> DateModified { get; set; } = DefaultDate;

        /// <summary>
        /// Formats a file size. The default scales to KB, MB and up, and shows <c>--</c> for
        /// something with no size, such as a folder.
        /// </summary>
        public Func<long, string> Size { get; set; } = DefaultSize;

        /// <summary>Legend entry for moving through the list.</summary>
        public Func<string> HintMove { get; set; } = static () => "move";

        /// <summary>Legend entry for entering the folder under the cursor.</summary>
        public Func<string> HintOpen { get; set; } = static () => "open";

        /// <summary>Legend entry for going to the parent folder.</summary>
        public Func<string> HintUp { get; set; } = static () => "up";

        /// <summary>Legend entry for switching to the sidebar of places.</summary>
        public Func<string> HintPlaces { get; set; } = static () => "places";

        /// <summary>Legend entry for opening a folder while picking folders.</summary>
        public Func<string> HintOpenFolder { get; set; } = static () => "open folder";

        /// <summary>Legend entry for the key that opens a folder or picks a file.</summary>
        public Func<string> HintOpenFolderOrPickFile { get; set; } = static () => "open folder / pick file";

        /// <summary>Legend entry for filtering by typing.</summary>
        public Func<string> HintFilter { get; set; } = static () => "filter";

        /// <summary>Legend entry for picking the folder that is open.</summary>
        public Func<string> HintPickCurrentFolder { get; set; } = static () => "pick this folder";

        /// <summary>Legend entry for leaving without picking anything.</summary>
        public Func<string> HintCancel { get; set; } = static () => "cancel";

        private static string DefaultKind(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                "" => "Document",
                ".zip" => "ZIP archive",
                ".7z" or ".rar" or ".gz" or ".tar" or ".bz2" => "Archive",
                ".md" => "Markdown document",
                ".txt" or ".log" => "Plain text",
                ".json" => "JSON document",
                ".toml" or ".yaml" or ".yml" or ".ini" or ".cfg" => "Configuration",
                ".xml" or ".html" or ".htm" => "Markup document",
                ".cs" or ".rs" or ".c" or ".cpp" or ".h" or ".py" or ".js" or ".ts" => "Source file",
                ".sh" or ".bash" => "Shell script",
                ".cmd" or ".bat" or ".ps1" => "Script",
                ".exe" or ".app" => "Application",
                ".dll" or ".so" or ".dylib" => "Library",
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".tga" or ".dds" or ".webp" => "Image",
                ".mp3" or ".wav" or ".ogg" or ".flac" => "Audio",
                ".mp4" or ".mkv" or ".avi" or ".mov" => "Movie",
                ".pdf" => "PDF document",
                _ => extension.TrimStart('.').ToUpperInvariant() + " file",
            };
        }

        private static string DefaultDate(DateTime value)
        {
            var today = DateTime.Today;
            var time = value.ToString("H:mm", CultureInfo.InvariantCulture);

            if (value.Date == today)
            {
                return $"Today at {time}";
            }

            if (value.Date == today.AddDays(-1))
            {
                return $"Yesterday at {time}";
            }

            return value.ToString("d MMM yyyy", CultureInfo.InvariantCulture) + " at " + time;
        }

        private static string DefaultSize(long bytes)
        {
            if (bytes < 0)
            {
                return "--";
            }

            string[] units = ["B", "KB", "MB", "GB", "TB"];
            double value = bytes;
            var unit = 0;

            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }

            return unit == 0
                ? $"{bytes} {units[0]}"
                : value.ToString(value >= 100 ? "0" : "0.0", CultureInfo.InvariantCulture) + " " + units[unit];
        }
    }
}
