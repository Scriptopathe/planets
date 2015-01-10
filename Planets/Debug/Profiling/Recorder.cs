// Copyright (C) 2013, 2014 Alvarez Josué
//
// This code is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2.1 of the License, or (at
// your option) any later version.
//
// This code is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
// License (LICENSE.txt) for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation,
// Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// The developer's email is jUNDERSCOREalvareATetudDOOOTinsa-toulouseDOOOTfr (for valid email, replace 
// capital letters by the corresponding character)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.PerformanceData;
using System.Diagnostics;
namespace Modouv.Fractales.Debug.Profiling
{
    /// <summary>
    /// Permet d'enregistrer des actions et de voir le temps pris par ces dernières.
    /// </summary>
    public class Recorder
    {
        class RecordData
        {
            public DateTime StartTime;
            public DateTime EndTime;
            public string ElapsedTime
            {
                get
                {
                    TimeSpan span = EndTime - StartTime;
                    return span.TotalMilliseconds.ToString() + " ms";
                }
            }
        }

          
        /// <summary>
        /// Stocke les données d'enregistrements.
        /// </summary>
        Dictionary<string, RecordData> m_record;

        /// <summary>
        /// Initialise une nouvelle instance de Recorder.
        /// </summary>
        public Recorder()
        {
            m_record = new Dictionary<string, RecordData>();
        }

        /// <summary>
        /// Supprime tous les enregistrements effectués.
        /// </summary>
        public void Clear()
        {
            m_record.Clear();
        }
        /// <summary>
        /// Commence l'enregistrement d'une tâche.
        /// </summary>
        /// <param name="recordName"></param>
        public void StartRecord(string recordName)
        {
            RecordData data = new RecordData();
            data.StartTime = DateTime.Now;
            m_record.Add(recordName, data);
        }

        /// <summary>
        /// Termine l'enregistrement d'une tâche.
        /// </summary>
        /// <param name="recordName"></param>
        public void EndRecord(string recordName)
        {
            m_record[recordName].EndTime = DateTime.Now;
        }
    }
}
