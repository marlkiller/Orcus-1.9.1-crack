using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.Resources;
using Orcus.Administration.ViewModels.About;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels
{
    public class AboutViewModel : PropertyChangedBase
    {
        private RelayCommand _navigateCommand;
        private RelayCommand _openLicenseCommand;

        public AboutViewModel()
        {
            Components = new List<Component>
            {
                new Component
                {
                    Name = "CSCore",
                    Url = "https://github.com/filoe/cscore",
                    LicenseInfo = LicenseInfo.MSPL,
                    Description = "CSCore is a free .NET audio library which is completely written in C#."
                },
                new Component
                {
                    Name = "AvalonEdit",
                    Url = "http://avalonedit.net/",
                    LicenseInfo = new MITLicenseInfo(""),
                    Description = "AvalonEdit is a WPF-based text editor component."
                },
                new Component
                {
                    Name = "MahApps.Metro",
                    Url = "http://mahapps.com/",
                    LicenseInfo = LicenseInfo.MSPL,
                    Description = "A toolkit for creating metro-style WPF applications."
                },
                new Component
                {
                    Name = "Json.NET",
                    Url = "http://james.newtonking.com/json",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2007 James Newton-King"),
                    Description =
                        "Popular high-performance JSON framework for .NET"
                },
                new Component
                {
                    Name = "Ookii.Dialogs",
                    Url = "http://www.ookii.org/Software/Dialogs/",
                    LicenseInfo = new LicenseInfo {Name = "BSD", Text = Licenses.Ookii_Dialogs},
                    Description =
                        "Ookii.Dialogs is a class library for .Net applications providing several common dialogs. "
                },
                new Component
                {
                    Name = "Mono.Cecil",
                    Url = "http://www.mono-project.com/Cecil",
                    LicenseInfo =
                        new MITLicenseInfo("Copyright (c) 2008 - 2015 Jb Evain Copyright (c) 2008 - 2011 Novell, Inc."),
                    Description =
                        "Cecil is a library to generate and inspect .NET programs and libraries. "
                },
                new Component
                {
                    Name = "ResourceLib",
                    Url = "https://github.com/dblock/resourcelib",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2008-2012 Daniel Doubrovkine, Vestris Inc."),
                    Description =
                        "C# File Resource Management Library"
                },
                new Component
                {
                    Name = "NetSerializer",
                    Url = "https://github.com/tomba/netserializer",
                    LicenseInfo = LicenseInfo.MPL2,
                    Description =
                        "Fast(est?) .Net Serializer"
                },
                new Component
                {
                    Name = "Fody",
                    Url = "https://github.com/Fody/Fody",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) Simon Cropp and contributors"),
                    Description =
                        "Extensible tool for weaving .net assemblies"
                },
                new Component
                {
                    Name = "Be.HexEditor",
                    Url = "http://sourceforge.net/projects/hexbox/",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2011 Bernhard Elbl"),
                    Description =
                        "HEX editor control for Windows Forms"
                },
                new Component
                {
                    Name = "ListView Layout Manager",
                    Url = "http://www.codeproject.com/Articles/25058/ListView-Layout-Manager",
                    LicenseInfo = LicenseInfo.CPOL,
                    Description =
                        "Using a ListViewLayoutManager allows controlling the behavior of the column layout of ListView/GridView controls"
                },
                new Component
                {
                    Name = "GridViewSort",
                    Url = "http://www.thomaslevesque.com/2009/08/04/wpf-automatically-sort-a-gridview-continued/",
                    Description = "Automatically sort a GridView"
                },
                new Component
                {
                    Name = "LZF",
                    Url = "https://csharplzfcompression.codeplex.com/",
                    LicenseInfo = new LicenseInfo {Name = "BSD", Text = Licenses.LZF_License},
                    Description = "A very small and extremely efficient real-time data compression library."
                },
                new Component
                {
                    Name = "AForge.Video",
                    Url = "http://www.aforgenet.com/",
                    LicenseInfo = LicenseInfo.LGPL,
                    Description =
                        "The AForge.Video library contains interfaces and classes to access different video sources, such as IP video cameras."
                },
                new Component
                {
                    Name = "StreamLibrary",
                    Url = "https://github.com/AnguisCaptor/StreamLibrary",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2016 AnguisCaptor"),
                    Description = "A library with codecs for Videos, Remote desktop, Animation and more"
                },
                new Component
                {
                    Name = "FirePwd.Net",
                    Url = "https://github.com/lclevy/firepwd",
                    Description = "Password reader for Mozilla Firefox and Thunderbird"
                },
                new Component
                {
                    Name = "Exceptionless",
                    Url = "https://exceptionless.com/",
                    LicenseInfo = LicenseInfo.Apache2,
                    Description = "Exceptionless provides real-time error reporting for your apps."
                },
                new Component
                {
                    Name = "Sparrow Toolkit",
                    Url = "https://sparrowtoolkit.codeplex.com/",
                    LicenseInfo = LicenseInfo.MSPL,
                    Description =
                        "Sparrow Toolkit is a set of Data Visualization controls (Chart, Gauge, BulletGraph and Sparkline)."
                },
                new Component
                {
                    Name = "Extended WPF Toolkit™",
                    Url = "https://wpftoolkit.codeplex.com/",
                    LicenseInfo = LicenseInfo.MSPL,
                    Description =
                        "Extended WPF Toolkit™ is the number one collection of WPF controls, components and utilities for creating next generation Windows applications."
                },
                new Component
                {
                    Name = "License System",
                    Url = "https://github.com/nikeee/license-system",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2015 Niklas Mollenhauer"),
                    Description =
                        "A license system with RSA signatures."
                },
                new Component
                {
                    Name = "nUpdate",
                    Url = "https://www.nupdate.net/",
                    LicenseInfo = LicenseInfo.MPL1_1,
                    Description =
                        "A modern update system for .Net applications."
                },
                new Component
                {
                    Name = "OxyPlot",
                    Url = "http://oxyplot.org/",
                    Description = "OxyPlot is a cross-platform plotting library for .NET.",
                    LicenseInfo = new MITLicenseInfo(null)
                },
                new Component
                {
                    Name = "GongSolutions.WPF.DragDrop",
                    Url = "https://github.com/punker76/gong-wpf-dragdrop",
                    Description = "The GongSolutions.WPF.DragDrop library is a drag'n'drop framework for WPF",
                    LicenseInfo = new BSDLicenseInfo("Copyright (c) 2015, Jan Karger (Steven Kirk)")
                },
                new Component
                {
                    Name = "NLog",
                    Url = "http://nlog-project.org/",
                    Description =
                        "NLog is a free logging platform for .NET with rich log routing and management capabilities",
                    LicenseInfo =
                        new BSDLicenseInfo(
                            "Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen")
                },
                new Component
                {
                    Name = "Starksoft-Aspen",
                    Url = "https://github.com/bentonstark/starksoft-aspen",
                    Description =
                        ".net security and cryptography library that provides client support for ftps, gnupg, smartcard, and socks / http proxies",
                    LicenseInfo = LicenseInfo.LGPL
                },
                new Component
                {
                    Name = "Task Scheduler Managed Wrapper",
                    Url = "https://taskscheduler.codeplex.com/",
                    Description = "This project provides a wrapper for the Windows Task Scheduler",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2003-2010 David Hall")
                },
                new Component
                {
                    Name = "DirectoryInfoEx",
                    Url = "https://directoryinfoex.codeplex.com/",
                    Description = "DirectoryInfoEx is a DirectoryInfo rewrite using IShellFolder",
                    LicenseInfo = LicenseInfo.LGPL
                },
                new Component
                {
                    Name = "File Explorer for WPF",
                    Url = "https://fileexplorer.codeplex.com/",
                    Description = "This project provides a lot of useful stuff for building a File Explorer",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2013 Leung Yat Chun Joseph")
                },
                new Component
                {
                    Name = "Opus .NET Wrapper",
                    Url = "https://github.com/JohnACarruthers/Opus.NET",
                    Description = "Wrapper for the audio codec Opus.",
                    LicenseInfo = new MITLicenseInfo("Copyright 2012 John Carruthers")
                },
                new Component
                {
                    Name = "Opus",
                    Url = "https://opus-codec.org/",
                    Description = "Opus is a totally open, royalty-free, highly versatile audio codec.",
                    LicenseInfo = new BSDLicenseInfo("")
                },
                new Component
                {
                    Name = "SharpDX",
                    Url = "http://sharpdx.org/",
                    Description = "SharpDX is an open-source managed .NET wrapper of the DirectX API.",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2010-2014 SharpDX - Alexandre Mutel")
                },
                new Component
                {
                    Name = "AS.TurboJpegWrapper",
                    Url = "http://sharpdx.org/",
                    Description = "Wrapper for libjpeg-turbo",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2015 Autonomic Systems")
                },
                new Component
                {
                    Name = "libjpeg-turbo",
                    Url = "http://libjpeg-turbo.virtualgl.org/",
                    Description = "libjpeg-turbo is a JPEG image codec [...] which is generally 2-6x as fast as libjpeg",
                    LicenseInfo = new BSDLicenseInfo("")
                },
                new Component
                {
                    Name = "WriteableBitmapEx",
                    Url = "https://github.com/teichgraf/WriteableBitmapEx/",
                    Description = "The WriteableBitmapEx library is a collection of extension methods for the WriteableBitmap.",
                    LicenseInfo = new MITLicenseInfo("Copyright (c) 2009-2015 Rene Schulte")
                }
            }.OrderBy(x => x.Name).ToList();

            ImageCreators = new List<ImageCreator>
            {
                new ImageCreator
                {
                    Name = "Designmodo",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3,
                    Website = "https://www.iconfinder.com/designmodo"
                },
                new ImageCreator
                {
                    Name = "Tahsin Tahil",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3,
                    Website = "https://www.iconfinder.com/tahsintahil"
                },
                new ImageCreator
                {
                    Name = "Freepik",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3,
                    Website = "http://www.flaticon.com/authors/freepik"
                },
                new ImageCreator
                {
                    Name = "Bogdan Rosu",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3,
                    Website = "http://www.flaticon.com/authors/bogdan-rosu"
                },
                new ImageCreator
                {
                    Name = "Yannick Lung",
                    Website = "https://www.iconfinder.com/yanlu"
                },
                new ImageCreator
                {
                    Name = "Design Revision",
                    Website = "https://www.iconfinder.com/DesignRevision"
                },
                new ImageCreator
                {
                    Name = "Timothy Miller",
                    LicenseInfo = LicenseInfo.CreateCommonsLicenseSa3,
                    Website = "https://www.iconfinder.com/tmthymllr"
                },
                new ImageCreator
                {
                    Name = "ionicons",
                    LicenseInfo = new MITLicenseInfo(""),
                    Website = "http://ionicons.com/"
                },
                new ImageCreator
                {
                    Name = "ProGlyphs",
                    Website = "https://www.iconfinder.com/milan.kohut"
                },
                new ImageCreator
                {
                    Name = "Paomedia",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3,
                    Website = "https://www.iconfinder.com/paomedia"
                },
                new ImageCreator
                {
                    Name = "Designerz Base",
                    Website = "https://www.iconfinder.com/Designerzbase"
                },
                new ImageCreator
                {
                    Name = "Recep Kütük",
                    Website = "https://www.iconfinder.com/recepkutuk"
                },
                new ImageCreator
                {
                    Name = "IconDrawer",
                    Website = "http://www.icondrawer.com/"
                },
                new ImageCreator
                {
                    Name = "Google",
                    Website = "https://www.google.com/"
                },
                new ImageCreator
                {
                    Name = "Squid.ink",
                    Website = "http://thesquid.ink/"
                },
                new ImageCreator
                {
                    Name = "Icons8",
                    Website = "https://icons8.com/",
                    LicenseInfo = LicenseInfo.CreateCommonsLicenseSa3
                },
                new ImageCreator
                {
                    Name = "Visual Studio Image Library",
                    Website = "https://www.microsoft.com/en-us/download/details.aspx?id=35825",
                    LicenseInfo =
                        new LicenseInfo
                        {
                            Name = "Visual Studio 2015 Image Library EULA",
                            Text = Licenses.Visual_Studio_2015_Image_Library_EULA
                        }
                }
            }.OrderByDescending(x => x.LicenseInfo != null).ThenBy(x => x.Name).ToList();

            AudioInfos = new List<AudioInfo>
            {
                new AudioInfo
                {
                    Creator = "CommanderDerp",
                    FileName = "Slender",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                },
                new AudioInfo
                {
                    Creator = "Benboncan",
                    FileName = "Fly",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                },
                new AudioInfo
                {
                    Creator = "Delilah",
                    FileName = "Tornado Siren II",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                },
                new AudioInfo
                {
                    Creator = "Mike Koenig",
                    FileName = "Zombie Horde",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                },
                new AudioInfo
                {
                    Creator = "thecheeseman",
                    FileName = "Camera Snap",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                },
                new AudioInfo
                {
                    Creator = "dingo1",
                    FileName = "Hell March",
                    LicenseInfo = LicenseInfo.CreateCommonsLicense3
                }
            };

            Translators = new List<Translator>
            {
                new Translator
                {
                    Name = "Rotta",
                    ProfileUrl = "https://hackforums.net/member.php?action=profile&uid=3206929",
                    LanguageName = "Finnish"
                },
                new Translator
                {
                    Name = "Baitable",
                    ProfileUrl = "https://hackforums.net/member.php?action=profile&uid=3320070",
                    LanguageName = "French"
                },
                new Translator
                {
                    Name = "AUDl",
                    ProfileUrl = "https://hackforums.net/member.php?action=profile&uid=2734270",
                    LanguageName = "Italian"
                },
                new Translator
                {
                    Name = "Layout",
                    ProfileUrl = "https://hackforums.net/member.php?action=profile&uid=2646762",
                    LanguageName = "Norwegian"
                }
            };

            var version = Assembly.GetEntryAssembly().GetName().Version;
            CurrentVersion = $"{version.Major}.{version.Minor}.{version.Build} (Build {version.Revision})";
        }

        public string CurrentVersion { get; }
        public List<ImageCreator> ImageCreators { get; }
        public List<Component> Components { get; }
        public List<AudioInfo> AudioInfos { get; }
        public List<Translator> Translators { get; }

        public RelayCommand NavigateCommand
        {
            get
            {
                return _navigateCommand ??
                       (_navigateCommand = new RelayCommand(parameter => { Process.Start(parameter.ToString()); }));
            }
        }

        public RelayCommand OpenLicenseCommand
        {
            get
            {
                return _openLicenseCommand ?? (_openLicenseCommand = new RelayCommand(parameter =>
                {
                    var licenseInfo = parameter as LicenseInfo;
                    if (licenseInfo == null)
                        return;

                    NotepadHelper.ShowMessage(licenseInfo.Text, licenseInfo.Name);
                }));
            }
        }
    }
}