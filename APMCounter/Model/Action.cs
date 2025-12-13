using System;
using System.Text.RegularExpressions;

namespace APMCounter.Model
{
    internal class Action
    {
        private int keycode;
        public int Keycode { get => keycode; set => keycode = value; }
        private string description;
        private string Description { get => description; set => description = value; }
        private DateTimeOffset timeOffset;
        public DateTimeOffset TimeOffset { get => timeOffset; set => timeOffset = value; }

        public Action() { }

        public Action(int keycode, string description, DateTimeOffset timeOffset)
        {
            Keycode = keycode;
            TimeOffset = timeOffset;
            Description = description;
        }

        public static Action FromString(string action) 
        {
            string pattern = @"Action\(keycode=(\d+),description=(.*),timeOffset=(.*)\)";
            Match match = Regex.Match(action, pattern);
            if (match.Success)
            {
                int keycode = int.Parse(match.Groups[1].Value);
                string description = match.Groups[2].Value;
                DateTimeOffset timeOffset = DateTimeOffset.Parse(match.Groups[3].Value);
                return new Action(keycode, description, timeOffset);
            }
            else 
            {
                throw new MissingMemberException(action);
            }
        }


        public override bool Equals(object obj)
        {
            return obj is Action action &&
                   keycode == action.keycode &&
                   timeOffset.Equals(action.timeOffset);
        }

        public override int GetHashCode()
        {
            int hashCode = 1212495600;
            hashCode = hashCode * -1521134295 + keycode.GetHashCode();
            hashCode = hashCode * -1521134295 + timeOffset.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"Action(keycode={keycode},description={description},timeOffset={timeOffset})";
        }
    }
}
