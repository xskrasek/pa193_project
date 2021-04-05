using System;
using System.Collections.Generic;
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

    class CommandLineOptions
    {
        private Dictionary<string, CommandLineOption> availableOptions = new Dictionary<string, CommandLineOption>();
        private CommandLineOption argumentOption;

        private ParsedOptions presentOptions = new ParsedOptions();

        private string helpOption;
        private string versionOption;
        private string versionString;

        public string executablePath { get; set; }

        public void AddOption(CommandLineOption option)
        {
            if (option == null) { throw new ArgumentNullException(); }
            if (option.OptionType == CommandLineOptionType.Argument)
            {
                if (argumentOption != null) { throw new ArgumentException($"Argument option is set already"); }
                argumentOption = option;
            }
            if (!this.availableOptions.ContainsKey(option.Name)) { this.availableOptions.Add(option.Name, option); }
        }

        public ParsedOptions Parse(string[] args)
        {
            if (args == null) { throw new ArgumentNullException(); }
            if (args.Length < 1) { throw new ArgumentException(); }

            // this.executablePath = args[0];
            // Apparently not on windows

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-") || arg.StartsWith("\\"))
                {
                    // Check if the option is supported
                    string[] splitSwitch = arg.Split(new string[] { "--", "-", "\\" }, StringSplitOptions.TrimEntries);
                    if (splitSwitch.Length != 2) { throw new ArgumentException($"Option {arg} is malformed"); }
                    if (!availableOptions.ContainsKey(splitSwitch[1])) { throw new ArgumentException($"Option {arg} is not supported"); }

                    string argName = splitSwitch[1];
                    CommandLineOption option = availableOptions[argName].Clone();
                    if (presentOptions.ContainsKey(argName)) { throw new ArgumentException($"Option {arg} is present already"); }

                    object value = null;

                    switch(option.OptionType)
                    {
                        case CommandLineOptionType.Option:
                            if (i + 1 >= args.Length) { throw new ArgumentException($"Option {arg} expects an argument"); }
                            string argument = args[i + 1];
                            i += 1; // Explicit is better than implicit
                            value = argument;
                            break;

                        case CommandLineOptionType.Switch:
                            // IDK if switches need any special treatment, they will simply be included in the presentOptions set
                            value = true;
                            break;

                        case CommandLineOptionType.Argument:
                            throw new ArgumentException($"Option {arg} should be used as an argument without any switch");
                    }

                    if (!presentOptions.ContainsKey(option.Name)) { presentOptions.Add(option.Name, value); }
                }
                else // Handling arguments without - or / prefixes
                {
                    if (argumentOption.Value == null) { argumentOption.Value = new List<string>(); }
                    ((List<string>)argumentOption.Value).Add(arg);
                }
            }
            if (argumentOption.Value != null) { presentOptions.Add(argumentOption.Name, argumentOption.Value); }
            return presentOptions;
        }

        public string GetHelp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Usage: ");
            foreach (var option in availableOptions.Values)
            {
                sb.Append("\t");
                switch (option.OptionType)
                {
                    case CommandLineOptionType.Switch:
                        sb.Append($"-{option.Name}");
                        break;
                    case CommandLineOptionType.Option:
                        sb.Append($"-{option.Name} <argument>");
                        break;
                    case CommandLineOptionType.Argument:
                        sb.Append($"<argument...>");
                        break;
                }
                sb.AppendLine($"\t {option.Description}");
            }
            return sb.ToString();
        }

        internal void SetHelpOption(string v)
        {
            this.helpOption = v;
        }

        internal void SetVersionOption(string v, string versionString)
        {
            this.versionOption = v;
            this.versionString = versionString;
        }
    }
}
