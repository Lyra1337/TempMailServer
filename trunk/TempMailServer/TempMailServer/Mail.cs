﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lyralabs.Net.TempMailServer
{
  class Mail
  {
    private static readonly Regex contentTypeParser = new Regex("(?<type>(multipart[^ ]+)) .*?boundary=(?<boundary>([^ ]+))", RegexOptions.Compiled);

    private string rawContent = null;
    private StringBuilder body = null;
    private Dictionary<string, List<string>> headers = null;

    public Mail(string _rawContent)
    {
      if (String.IsNullOrEmpty(_rawContent))
        throw new ArgumentNullException("rawContent is null!");

      this.rawContent = _rawContent;
      this.ParseHeader();
      this.ParseBody();
    }

    private void ParseBody()
    {
      string contentType = null;
      if (this.headers.ContainsKey("Content-Type"))
      {
        List<string> c = this.headers["Content-Type"];
        if (c != null && c.Count > 0)
        {
          Match m = Mail.contentTypeParser.Match(c[0]);
          if (m.Success)
          {
            string boundary = m.Groups["boundary"].Value;
            contentType = m.Groups["type"].Value;
          }
        }
      }
    }

    private void ParseHeader()
    {
      this.body = new StringBuilder();
      bool header = true;
      this.headers = new Dictionary<string, List<string>>();

      foreach (string line in this.rawContent.Replace("\r", "").Split('\n'))
      {
        if (header)
        {
          if (String.IsNullOrEmpty(line))
          {
            header = false;
          }
          else if (line.StartsWith(" ") && this.headers.Count > 0)
          {
            this.headers[this.headers.Keys.Last()][this.headers[this.headers.Keys.Last()].Count - 1] += line.Substring(1, line.Length - 1).Trim();
          }
          else
          {
            string key, value;
            int separator = line.IndexOf(':');
            key = line.Substring(0, separator);
            value = line.Substring(separator + 1, line.Length - (separator + 1)).TrimStart();
            if (this.headers.ContainsKey(key) == false)
            {
              this.headers.Add(key, new List<string>());
            }
            this.headers[key].Add(value);
          }
        }
        else
        {
          this.body.AppendLine(line);
        }
      }
    }
  }
}