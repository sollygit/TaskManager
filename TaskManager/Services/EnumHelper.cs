using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskManager.Services
{
    public class EnumHelper : IEnumHelperService
    {
        public EnumHelper()
        {
        }

        public Dictionary<int, string> MapEnumToDictionary<T>()
        {
            // Ensure T is an enumerator
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerator type.");
            }

            // Return Enumertator as a Dictionary
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(i => (int)Convert.ChangeType(i, i.GetType()), t => t.ToString());
        }
    }

    public interface IEnumHelperService
    {
        Dictionary<int, string> MapEnumToDictionary<T>();
    }
}
