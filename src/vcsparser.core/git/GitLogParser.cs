﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.git
{
    public class GitLogParser : IGitLogParser
    {
        public readonly String COMMIT_PREFIX = "commit ";
        public readonly String AUTHOR_PREFIX = "Author: ";
        public readonly String AUTHOR_DATE_PREFIX = "AuthorDate: ";
        public readonly String COMMITER_PREFIX = "Commit: ";
        public readonly String COMMIT_DATE_PREFIX = "CommitDate: ";
        public readonly String MERGE_PREFIX = "Merge: ";

        public List<GitCommit> Parse(List<string> lines)
        {
            GitLogParserContext context = new GitLogParserContext();
            foreach (var line in lines)
            {
                ParseLine(context, line);
            }
            return context.Commits;
        }

        private void ParseLine(GitLogParserContext context, String line)
        {
            if (line.StartsWith(COMMIT_PREFIX))
            {
                context.CurrentCommit = new GitCommit();
                context.Commits.Add(context.CurrentCommit);
                context.CurrentState = GitLogParserContext.State.NewCommit;
                context.CurrentCommit.CommitHash = GetValue(COMMIT_PREFIX, line);
            }
            else if (line.StartsWith(AUTHOR_PREFIX))
            {
                context.CurrentCommit.Author = GetValue(AUTHOR_PREFIX, line);
            }
            else if (line.StartsWith(AUTHOR_DATE_PREFIX))
            {
                context.CurrentCommit.AuthorDate = Iso8601StringToDateTime(GetValue(AUTHOR_DATE_PREFIX, line));
            }
            else if (line.StartsWith(COMMITER_PREFIX))
            {
                context.CurrentCommit.Commiter = GetValue(COMMITER_PREFIX, line);
            }
            else if (line.StartsWith(COMMIT_DATE_PREFIX))
            {
                context.CurrentCommit.CommiterDate = Iso8601StringToDateTime(GetValue(COMMIT_DATE_PREFIX, line));
            }
            else if (line == "")
            {
                UpdateState(context);
            }
            else if (!IsIgnoredLine(line))
            {
                ParseStateDependentLine(context, line);
            }
        }

        private bool IsIgnoredLine(string line)
        {
            return line.StartsWith(MERGE_PREFIX);                            
        }

        public DateTime Iso8601StringToDateTime(string isoDateTime)
        {
            return DateTime.Parse(isoDateTime, null, DateTimeStyles.RoundtripKind);
        }

        private String GetValue(String prefix, String line)
        {
            return line.Substring(prefix.Length).Trim();
        }

        private void UpdateState(GitLogParserContext context)
        {
            switch (context.CurrentState)
            {
                case GitLogParserContext.State.NewCommit:
                    context.CurrentState = GitLogParserContext.State.ParsingDescription;
                    break;
                case GitLogParserContext.State.ParsingDescription:
                    context.CurrentState = GitLogParserContext.State.ParsingStats;
                    break;
                default: //GitLogParserContext.State.ParsingStats
                    context.CurrentState = GitLogParserContext.State.NewCommit;
                    break;
            }
        }

        private void ParseStateDependentLine(GitLogParserContext context, String line)
        {
            if (context.CurrentState == GitLogParserContext.State.ParsingDescription)
                context.CurrentCommit.AppendCommitMessage(line.Trim());
            else 
                // context.CurrentState == GitLogParserContext.State.ParsingStats
                ParseStatsLine(context, line);                    
        }

        private void ParseStatsLine(GitLogParserContext context, String line)
        {
            String[] stats = line.Split('\t');
            FileChanges file = new FileChanges();
            file.Added = Convert.ToInt32(stats[0].Replace('-', '0'));
            file.Deleted = Convert.ToInt32(stats[1].Replace('-', '0'));
            file.FileName = ProcessRenames(context, stats[2]);
            context.CurrentCommit.ChangesetFileChanges.Add(file);
        }

        public string ProcessRenames(GitLogParserContext context, string fileName)
        {
            if (!fileName.Contains("=>"))
                return fileName;

            string stringToReplace = fileName;
            string stringWithBothValues = fileName;
            if (fileName.IndexOf("{") >= 0)
            {          
                stringToReplace = fileName.Substring(fileName.IndexOf("{"), fileName.IndexOf("}") - fileName.IndexOf("{") + 1).Trim();
                stringWithBothValues = stringToReplace.Substring(1, stringToReplace.Length - 2);
            }            
            string oldFileName = stringWithBothValues.Substring(0, stringWithBothValues.IndexOf("=>")).Trim();
            string newFileName = stringWithBothValues.Substring(stringWithBothValues.IndexOf("=>") + 2).Trim();

            oldFileName = fileName.Replace(stringToReplace, oldFileName).Replace("//", "/");
            newFileName = fileName.Replace(stringToReplace, newFileName).Replace("//", "/");

            context.CurrentCommit.ChangesetFileRenames.Add(oldFileName, newFileName);

            return newFileName;
        }
    }
}
