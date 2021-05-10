using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PA193_Project.CommandLine
{
    enum CommandLineOptionType { Option, Argument, Switch }

    class CommandLineOption
    {
        public CommandLineOption(string name, CommandLineOptionType optionType, string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            OptionType = optionType;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public CommandLineOptionType OptionType { get; }
        public string Name { get; }
        public string Description { get; }
        public object Value { get; set; }

        internal CommandLineOption Clone()
        {
            return (CommandLineOption) this.MemberwiseClone();
        }
    }

    class ParsedOptions
    {
        private Dictionary<string, object> storage = new Dictionary<string, object>();

        internal bool ContainsKey(string argName)
        {
            return storage.ContainsKey(argName);
        }

        internal void Add(string name, object argumentOption)
        {
            if (!storage.ContainsKey(name)) { storage.Add(name, argumentOption); }
        }

        internal T Get<T>(string key)
        {
            if (storage.ContainsKey(key))
                return (T)storage[key];
            else return default;
        }

        internal bool IsEmpty() { return storage.Count == 0; }
    }

    [Serializable]
    public class CommandLineArgumentException : Exception
    {
        public CommandLineArgumentException() { }
        public CommandLineArgumentException(string message) : base(message) { }
        public CommandLineArgumentException(string message, Exception innerException) : base(message, innerException) { }
        protected CommandLineArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    class CommandLineOptions
    {
        private Dictionary<string, CommandLineOption> _availableOptions = new Dictionary<string, CommandLineOption>();
        private CommandLineOption _argumentOption;

        private ParsedOptions _presentOptions = new ParsedOptions();

        public string ExecutablePath { get; set; }

        public void AddOption(CommandLineOption option)
        {
            if (option == null) { throw new ArgumentNullException(); }
            if (option.OptionType == CommandLineOptionType.Argument)
            {
                if (_argumentOption != null) { throw new CommandLineArgumentException($"Argument option is set already"); }
                _argumentOption = option;
            }
            if (!this._availableOptions.ContainsKey(option.Name)) { this._availableOptions.Add(option.Name, option); }
        }

        public ParsedOptions Parse(string[] args)
        {
            if (args == null) { throw new ArgumentNullException(); }
            if (args.Length < 1) { throw new CommandLineArgumentException(); }

            // this.executablePath = args[0];
            // Apparently not on windows

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-") || arg.StartsWith("\\"))
                {
                    // Check if the option is supported
                    string[] splitSwitch = arg.Split(new[] { "--", "-", "\\" }, StringSplitOptions.TrimEntries);
                    if (splitSwitch.Length != 2) { throw new CommandLineArgumentException($"Option {arg} is malformed"); }
                    if (!_availableOptions.ContainsKey(splitSwitch[1])) { throw new CommandLineArgumentException($"Option {arg} is not supported"); }

                    string argName = splitSwitch[1];
                    CommandLineOption option = _availableOptions[argName].Clone();
                    if (_presentOptions.ContainsKey(argName)) { throw new CommandLineArgumentException($"Option {arg} is present already"); }

                    object value = null;

                    switch(option.OptionType)
                    {
                        case CommandLineOptionType.Option:
                            if (i + 1 >= args.Length) { throw new CommandLineArgumentException($"Option {arg} expects an argument"); }
                            string argument = args[i + 1];
                            i += 1; // Explicit is better than implicit
                            value = argument;
                            break;

                        case CommandLineOptionType.Switch:
                            // IDK if switches need any special treatment, they will simply be included in the presentOptions set
                            value = true;
                            break;

                        case CommandLineOptionType.Argument:
                            throw new CommandLineArgumentException($"Option {arg} should be used as an argument without any switch");
                    }

                    if (!_presentOptions.ContainsKey(option.Name)) { _presentOptions.Add(option.Name, value); }
                }
                else // Handling arguments without - or / prefixes
                {
                    if (_argumentOption.Value == null) { _argumentOption.Value = new List<string>(); }
                    ((List<string>)_argumentOption.Value).Add(arg);
                }
            }
            if (_argumentOption.Value != null) { _presentOptions.Add(_argumentOption.Name, _argumentOption.Value); }
            return _presentOptions;
        }

        public string GetHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage: ");
            foreach (var option in _availableOptions.Values)
            {
                string prefix = (option.Name.Length == 1) ? "-" : "--";

                sb.Append('\t');
                switch (option.OptionType)
                {
                    case CommandLineOptionType.Switch:
                        sb.Append($"{prefix}{option.Name}");
                        break;
                    case CommandLineOptionType.Option:
                        sb.Append($"{prefix}{option.Name} <argument>");
                        break;
                    case CommandLineOptionType.Argument:
                        sb.Append($"<argument...>");
                        break;
                }
                sb.AppendLine($"\t {option.Description}");
            }
            return sb.ToString();
        }
    }
}
