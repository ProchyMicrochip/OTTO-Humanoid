#pragma warning disable IDE0073
// Copyright 2019 The MediaPipe Authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text.RegularExpressions;

namespace Mediapipe.Net.Framework.Tool
{
    /// <summary>
    ///   translated version of mediapipe/framework/tool/validate_name.cc
    /// <summary/>
    internal static partial class Internal
    {
        public const int MaxCollectionItemId = 10000;
    }

    public static partial class Tool
    {
        private const string name_regex = "[a-z_][a-z0-9_]*";
        private const string number_regex = "(0|[1-9][0-9]*)";
        private const string tag_regex = "[A-Z_][A-Z0-9_]*";
        private static readonly string tagAndNameRegex = $"({tag_regex}:)?{name_regex}";
        private static readonly string tagIndexNameRegex = $"({tag_regex}:({number_regex}:)?)?{name_regex}";
        private static readonly string tagIndexRegex = $"({tag_regex})?(:{number_regex})?";

        public static void ValidateName(string name)
        {
            if (name.Length > 0 && new Regex($"^{name_regex}$").IsMatch(name))
                return;

            throw new ArgumentException($"Name \"{name}\" does not match \"{name_regex}\".");
        }

        public static void ValidateNumber(string number)
        {
            if (number.Length > 0 && new Regex($"^{number_regex}$").IsMatch(number))
                return;

            throw new ArgumentException($"Number \"{number}\" does not match \"{number_regex}\".");
        }

        public static void ValidateTag(string tag)
        {
            if (tag.Length > 0 && new Regex($"^{tag_regex}$").IsMatch(tag))
                return;

            throw new ArgumentException($"Tag \"{tag}\" does not match \"{tag_regex}\".");
        }

        public static void ParseTagAndName(string tagAndName, out string tag, out string name)
        {
            int nameIndex;
            string[] v = tagAndName.Split(':');

            try
            {
                if (v.Length == 1)
                {
                    ValidateName(v[0]);
                    nameIndex = 0;
                }
                else if (v.Length == 2)
                {
                    ValidateTag(v[0]);
                    ValidateName(v[1]);
                    nameIndex = 1;
                }
                else
                {
                    throw new ArgumentException("nope");
                }

            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"\"tag and name\" is invalid, \"{tagAndName}\" does not match \"{tagAndNameRegex}\" (examples: \"TAG:name\", \"longer_name\").");
            }

            tag = nameIndex == 1 ? v[0] : "";
            name = v[nameIndex];
        }

        public static void ParseTagIndexName(string tagIndexName, out string tag, out int index, out string name)
        {
            int nameIndex;
            int theIndex = 0;
            string[] v = tagIndexName.Split(':');

            try
            {
                if (v.Length == 1)
                {
                    ValidateName(v[0]);
                    theIndex = -1;
                    nameIndex = 0;
                }
                else if (v.Length == 2)
                {
                    ValidateTag(v[0]);
                    ValidateName(v[1]);
                    nameIndex = 1;
                }
                else if (v.Length == 3)
                {
                    ValidateTag(v[0]);
                    ValidateNumber(v[1]);

                    if (int.TryParse(v[1], out var result) && result <= Internal.MaxCollectionItemId)
                        theIndex = result;
                    else
                        throw new ArgumentException("nope");

                    ValidateName(v[2]);
                    nameIndex = 2;
                }
                else
                {
                    throw new ArgumentException("nope");
                }
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"TAG:index:name is invalid, \"{tagIndexName}\" does not match \"{tagIndexNameRegex}\" (examples: \"TAG:name\", \"VIDEO:2:name_b\", \"longer_name\").");
            }

            tag = nameIndex != 0 ? v[0] : "";
            index = theIndex;
            name = v[nameIndex];
        }

        public static void ParseTagIndex(string tagIndex, out string tag, out int index)
        {
            int theIndex;
            string[] v = tagIndex.Split(':');

            try
            {
                if (v.Length == 1)
                {
                    if (v[0].Length != 0)
                        ValidateTag(v[0]);
                    theIndex = 0;
                }
                else if (v.Length == 2)
                {
                    if (v[0].Length != 0)
                        ValidateTag(v[0]);
                    ValidateNumber(v[1]);

                    if (int.TryParse(v[1], out var result) && result <= Internal.MaxCollectionItemId)
                        theIndex = result;
                    else
                        throw new ArgumentException("nope");
                }
                else
                {
                    throw new ArgumentException("nope");
                }
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"TAG:index is invalid, \"{tagIndex}\" does not match \"{tagIndexRegex}\" (examples: \"TAG\", \"VIDEO:2\").");
            }

            tag = v[0];
            index = theIndex;
        }
    }
}
#pragma warning restore IDE0073
