using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SimpleTriangle.Debug
{
    public partial class DiagnosisWindow : Form
    {
        /// <summary>
        /// Définit le nombre de threads en cours d'exécution.
        /// </summary>
        public int ThreadCount
        {
            set { m_threadsCountTextbox.Text = value.ToString(); }
        }
        /// <summary>
        /// Définit le nombre de threads en attente.
        /// </summary>
        public int WaitingThreadCount
        {
            set { m_waitingThreadsTextbox.Text = value.ToString(); }
        }
        /// <summary>
        /// Crée une nouvelle instance de Diagnosis Window.
        /// </summary>
        public DiagnosisWindow()
        {
            InitializeComponent();
        }
    }
}
