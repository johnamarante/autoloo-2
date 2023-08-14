using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class AutolooUserInfoUtil
{
    private static Dictionary<char, string> UserDirectoryURLs = new Dictionary<char, string>
    {
        {'a', "https://friendpaste.com/6cn5e2zpveZ74sskLcomWq"},
        {'b', "https://friendpaste.com/6cn5e2zpveZ74sskLd67XG"},
        {'c', "https://friendpaste.com/6cn5e2zpveZ74sskLdEf6N"},
        {'d', "https://friendpaste.com/6cn5e2zpveZ74sskLdEdw9"},
        {'e', "https://friendpaste.com/1we5GdORGcM58V3cMGbTOg"},
        {'f', "https://friendpaste.com/1we5GdORGcM58V3cMGbRk4"},
        {'0', "https://friendpaste.com/6cn5e2zpveZ74sskLdEclx"},
        {'1', "https://friendpaste.com/6cn5e2zpveZ74sskLdEbq9"},
        {'2', "https://friendpaste.com/6cn5e2zpveZ74sskLdEaan"},
        {'3', "https://friendpaste.com/6cn5e2zpveZ74sskLdEZkj"},
        {'4', "https://friendpaste.com/6cn5e2zpveZ74sskLdEYgx"},
        {'5', "https://friendpaste.com/6cn5e2zpveZ74sskLdEXfM"},
        {'6', "https://friendpaste.com/6cn5e2zpveZ74sskLd664N"},
        {'7', "https://friendpaste.com/1we5GdORGcM58V3cMGbQ9o"},
        {'8', "https://friendpaste.com/6cn5e2zpveZ74sskLd654B"},
        {'9', "https://friendpaste.com/1we5GdORGcM58V3cMGbOm2"}
    };

    public static string GetUserDirectoryURLBasedOnUserID(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string cannot be null or empty.");
        }

        char lastChar = char.ToLower(input[input.Length - 1]); // Get the last character of the input string

        if (UserDirectoryURLs.ContainsKey(lastChar))
        {
            return UserDirectoryURLs[lastChar];
        }
        else
        {
            throw new ArgumentException("Input string does not correspond to a valid URL.");
        }
    }

}
