﻿//
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
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace DeployAfterBuild.Settings
{
    internal sealed class OptionPageGrid : DialogPage
    {
        private bool m_Enabled = false;

        [Category("Deploy")]
        [DisplayName("1. Enable")]
        [Description("Enable the deploy feature")]
        public bool Enabled
        {
            get
            {
                return m_Enabled;
            }
            set
            {
                m_Enabled = value;
            }
        }

        [Category("Deploy")]
        [DisplayName("2. Destination Path")]
        [Description("Path where the files should be deployed to")]
        public string DestinationPath { get; set; }
    }
}
