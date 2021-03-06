﻿using CommandLine;

namespace AnyBar.CLI
{
    public class Options
    {
        [Option('h', "host", Default = "127.0.0.1", Required = false,
            HelpText = "The host name or IP address of the machine running the AnyBar instance")]
        public string Host { get; set; }

        [Option('p', "port", Default = AnyBarClient.DEFAULT_PORT, Required = false,
            HelpText = "The port number that the AnyBar instance is listening on")]
        public int Port { get; set; }

        [Option('c', "color", Required = true,
            HelpText = "Color to set on AnyBar instance")]
        public AnyBarImage Color { get; set; }
    }
}
