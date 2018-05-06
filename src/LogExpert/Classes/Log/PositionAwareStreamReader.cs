﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Linq;
using System.Text;
using System.IO;
using LogExpert.Classes.Log;

namespace LogExpert
{
    /// <summary>
    /// This class is responsible for reading line from the log file. It also decodes characters with the appropriate charset encoding.
    /// PositionAwareStreamReader tries a BOM detection to determine correct file offsets when directly seeking into the file (on re-loading flushed buffers).
    /// UTF-8 handling is a bit slower, because after reading a character the byte length of the character must be determined.
    /// Lines are read char-by-char. StreamReader.ReadLine() is not used because StreamReader cannot tell a file position.
    /// </summary>
    public class PositionAwareStreamReader : PositionAwareStreamReaderBase
    {
        #region Fields

        protected int _newLineSequenceLength;

        #endregion

        #region cTor

        public PositionAwareStreamReader(Stream stream, EncodingOptions encodingOptions)
            : base(stream, encodingOptions)
        {
            _newLineSequenceLength = GuessNewLineSequenceLength();
        }

        #endregion

        #region Public methods

        public override string ReadLine()
        {
            if (_newLineSequenceLength == 0)
            {
                _newLineSequenceLength = GuessNewLineSequenceLength();
            }

            string line = _reader.ReadLine();
            if (line != null)
            {
                _pos += Encoding.GetByteCount(line);

                _pos += _newLineSequenceLength;

                if (line.Length > MAX_LINE_LEN)
                {
                    line = line.Remove(MAX_LINE_LEN);
                }
            }

            return line;
        }

        #endregion

        protected int GuessNewLineSequenceLength()
        {
            long currentPos = Position;
            int len = 0;
            string line = _reader.ReadLine();
            if (line != null)
            {
                _stream.Seek(Encoding.GetByteCount(line) + _preambleLength, SeekOrigin.Begin);
                ResetReader();
                int b = _reader.Read();
                // int b = this.stream.ReadByte();
                if (b == 0x0d)
                {
                    // b = this.stream.ReadByte();
                    b = _reader.Read();
                    if (b == 0x0a)
                    {
                        len = 2;
                    }
                    else
                    {
                        len = 1;
                    }
                }
                else
                {
                    len = 1;
                }

                len *= Encoding.GetByteCount("\r");
            }

            Position = currentPos;
            return len;
        }
    }
}