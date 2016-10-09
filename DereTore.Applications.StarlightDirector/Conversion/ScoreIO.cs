﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using DereTore.Applications.StarlightDirector.Conversion.Formats.Deleste;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.Conversion {
    public static class ScoreIO {

        public static Score LoadFromDelesteBeatmap(Project temporaryProject, Difficulty difficulty, string fileName, out string[] warnings) {
            warnings = null;
            var encoding = DelesteHelper.TryDetectDelesteBeatmapEncoding(fileName);
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                using (var streamReader = new StreamReader(fileStream, encoding, true)) {
                    if (streamReader.EndOfStream) {
                        return null;
                    }
                    var score = new Score(temporaryProject, difficulty);
                    var noteCache = new List<DelesteBasicNote>();
                    var entryCache = new List<DelesteBeatmapEntry>();
                    var warningList = new List<string>();
                    var entryCounter = 0;
                    do {
                        var line = streamReader.ReadLine();
                        if (line.Length == 0 || line[0] != '#') {
                            continue;
                        }
                        ++entryCounter;
                        var entry = DelesteHelper.ReadEntry(temporaryProject, line, entryCounter, noteCache, warningList);
                        if (entry != null) {
                            entryCache.Add(entry);
                        }
                    } while (!streamReader.EndOfStream);
                    DelesteHelper.AnalyzeDelesteBeatmap(score, entryCache, warningList);
                    if (warningList.Count > 0) {
                        warnings = warningList.ToArray();
                    }
                    return score;
                }
            }
        }

    }
}
