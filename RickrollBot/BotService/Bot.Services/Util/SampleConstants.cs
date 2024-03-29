﻿using Microsoft.Skype.Bots.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Services.Util
{
    /// <summary>
    /// Contants used by the bot.
    /// </summary>
    public static class SampleConstants
    {
        /// <summary>
        /// Number of sockets to receive video only
        /// The main video socket being sendrecv this brings the total to NumberOfMultiviewSockets + 1 receive channels.
        /// </summary>
        public const uint NumberOfMultiviewSockets = 3;

        /// <summary>
        /// Stores a list of supported video formats.
        /// </summary>
        public static readonly List<VideoFormat> SupportedSendVideoFormats = new List<VideoFormat>
        {
            VideoFormat.H264_1280x720_30Fps,
            VideoFormat.H264_640x360_30Fps,
            VideoFormat.H264_320x180_15Fps,
        };
    }
}
