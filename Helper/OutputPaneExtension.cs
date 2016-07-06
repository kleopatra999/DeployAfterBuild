//
// Copyright 2016 David Roller
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using EnvDTE;
using System;

namespace DeployAfterBuild.Helper
{
    internal static class OutputPaneExtension
    {
        public static void OutputStringLine(this OutputWindowPane pane, string str)
        {
            string date = DateTime.Now.ToString();
            pane.OutputString(date + " [INFO]\t" + str + Environment.NewLine);
        }

        public static void OutputWarnLine(this OutputWindowPane pane, string str)
        {
            string date = DateTime.Now.ToString();
            pane.OutputString(date + " [WARN]\t" + str + Environment.NewLine);
        }

        public static void OutputErrorLine(this OutputWindowPane pane, string str)
        {
            string date = DateTime.Now.ToString();
            pane.OutputString(date + " [ERROR]\t" + str + Environment.NewLine);
        }
    }
}
