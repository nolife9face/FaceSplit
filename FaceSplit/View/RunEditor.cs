﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FaceSplit.Model;

namespace FaceSplit
{
    public partial class RunEditor : Form
    {
        private const int INVALID_VALUE = -1;

        private Split split;
        private int rowHeight;
        private int numberOfRows;

        public RunEditor(Split split)
        {
            InitializeComponent();
            this.txtAttemptsCount.Text = "0";
            if (split != null)
            {
                this.split = split;
                this.txtRunTitle.Text = this.split.RunTitle;
                this.txtRunGoal.Text = this.split.RunGoal;
                this.txtAttemptsCount.Text = this.split.AttemptsCount.ToString();
                FillSegmentRows();
            }
            rowHeight = this.segmentsGridView.RowTemplate.Height;
            AddRow();
            numberOfRows = this.segmentsGridView.Rows.Count;
        }

        public Split Split
        {
            get { return split; }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSplit();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            FillSegmentsTime();
        }

        private void txtAttemptsCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Proceed to add a row in the table view and set a higher 
        /// height to the window if needed.
        /// </summary>
        private void AddRow()
        {
            DataGridViewRow newRow = new DataGridViewRow();
            newRow.Height = rowHeight;
            this.segmentsGridView.Rows.Insert(numberOfRows, newRow);
            this.segmentsGridView.Height += rowHeight;
            numberOfRows++;
            if (numberOfRows > 1)
            {
                this.Height += rowHeight;
                MoveButtonsUnderGridView();
            }
        }

        /// <summary>
        /// When adding a row in the grid view, the buttons under it need to go down.
        /// This is what we do here.
        /// </summary>
        private void MoveButtonsUnderGridView()
        {
            btnInsert.Top += rowHeight;
            btnImport.Top += rowHeight;
            btnFill.Top += rowHeight;
            btnSave.Top += rowHeight;
            btnCancel.Top += rowHeight;
            lblAttemptsCount.Top += rowHeight;
            txtAttemptsCount.Top += rowHeight;
        }

        private void segmentsGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridViewTextBoxEditingControl gridTextChangeControl = (DataGridViewTextBoxEditingControl)e.Control;
            gridTextChangeControl.KeyPress += new KeyPressEventHandler(this.segmentGridView_KeyPress);
        }

        private void segmentGridView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.segmentsGridView.CurrentCell.ColumnIndex != 0)
            {
                if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && e.KeyChar != ':' && e.KeyChar != '.' && e.KeyChar != ',')
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Save split with the information in the editor.
        /// </summary>
        private void SaveSplit()
        {
            this.split = new Split();
            this.split.RunTitle = this.txtRunTitle.Text;
            this.split.RunGoal = this.txtRunGoal.Text;
            this.split.AttemptsCount = (String.IsNullOrEmpty(txtAttemptsCount.Text.Trim())) ? 0 : Int32.Parse(txtAttemptsCount.Text);
            FillSplitSegments();

        }

        private void FillSplitSegments()
        {
            foreach (DataGridViewRow rows in this.segmentsGridView.Rows)
            {
                String segmentName = (rows.Cells[0].Value == null) ? "-" : rows.Cells[0].Value.ToString();
                double splitTime = (rows.Cells[1].Value == null) ? 0.0 : FaceSplitUtils.TimeParse(rows.Cells[1].Value.ToString());
                double segmentTime = (rows.Cells[2].Value == null) ? 0.0 : FaceSplitUtils.TimeParse(rows.Cells[2].Value.ToString());
                double bestSegmentTime = (rows.Cells[3].Value == null) ? 0.0 : FaceSplitUtils.TimeParse(rows.Cells[3].Value.ToString());
                this.split.Segments.Add(new Segment(segmentName, splitTime, segmentTime, bestSegmentTime));
            }
        }

        /// <summary>
        /// Fill the rows in the table with the split information.
        /// </summary>
        private void FillSegmentRows()
        {
            for (int i = 0; i < this.split.Segments.Count; ++i)
            {
                if (i > 0)
                {
                    AddRow();
                }
                this.segmentsGridView.Rows[i].Cells[0].Value = this.split.Segments.ElementAt(i).SegmentName;
                this.segmentsGridView.Rows[i].Cells[1].Value = this.split.Segments.ElementAt(i).SplitTime;
                this.segmentsGridView.Rows[i].Cells[2].Value = this.split.Segments.ElementAt(i).SegmentTime;
                this.segmentsGridView.Rows[i].Cells[3].Value = this.split.Segments.ElementAt(i).BestSegmentTime;
            }
        }

        /// <summary>
        /// Fill the segment time and the best segment time column.
        /// </summary>
        private void FillSegmentsTime()
        {
            List<double> splitsTime = new List<double>();
            List<double> segmentsTime = new List<double>();

            FillSplitsTime(splitsTime);
            CalculateSegmentsTime(splitsTime, segmentsTime);
            FillSegmentColumns(segmentsTime); 
        }

        /// <summary>
        /// Get the list of splits time from the data grid view.
        /// </summary>
        /// <param name="splitsTime">The list of splits time to be fill.</param>
        private void FillSplitsTime(List<double> splitsTime)
        {
            foreach (DataGridViewRow rows in this.segmentsGridView.Rows)
            {
                String splitTimeString = (rows.Cells[1].Value == null) ? "" : rows.Cells[1].Value.ToString();
                if (!String.IsNullOrEmpty(splitTimeString.Trim()))
                {
                    splitsTime.Add(FaceSplitUtils.TimeParse(splitTimeString));
                }
                else
                {
                    splitsTime.Add(INVALID_VALUE);
                }
            }
        }

        /// <summary>
        /// Take the splits time and calculate segments time from them.
        /// </summary>
        /// <param name="splitsTime">The list of splits time.</param>
        /// <param name="segmentsTime">The list of segments time to be fill.</param>
        private void CalculateSegmentsTime(List<double> splitsTime, List<double> segmentsTime)
        {
            for (int i = 0; i < splitsTime.Count; ++i)
            {
                if (i > 0)
                {
                    if (splitsTime.ElementAt(i - 1) != INVALID_VALUE && splitsTime.ElementAt(i) != INVALID_VALUE)
                    {
                        double segmentTime = splitsTime.ElementAt(i) - splitsTime.ElementAt(i - 1);
                        segmentsTime.Add(segmentTime);
                    }
                    else
                    {
                        segmentsTime.Add(INVALID_VALUE);
                    }
                }
                else
                {
                    if (splitsTime.ElementAt(i) != INVALID_VALUE)
                    {
                        segmentsTime.Add(splitsTime.ElementAt(i));
                    }
                    else
                    {
                        segmentsTime.Add(INVALID_VALUE);
                    }
                }
            }
        }

        /// <summary>
        /// Fill the segment cells in the table with the segments time from the list.
        /// </summary>
        /// <param name="segmentsTime">The list we use to fill the cells.</param>
        private void FillSegmentColumns(List<double> segmentsTime)
        {
            int index = 0;
            foreach (DataGridViewRow rows in this.segmentsGridView.Rows)
            {
                DataGridViewCell cellSegmentTime = rows.Cells[2];
                DataGridViewCell cellBestSegmentTime = rows.Cells[3];
                if (segmentsTime.ElementAt(index) != INVALID_VALUE)
                {
                    cellSegmentTime.Value = FaceSplitUtils.TimeFormat(segmentsTime.ElementAt(index));
                    cellBestSegmentTime.Value = FaceSplitUtils.TimeFormat(segmentsTime.ElementAt(index));
                }
                else
                {
                    cellSegmentTime.Value = "";
                    cellBestSegmentTime.Value = "";
                }
                index++;
            }
        }
    }
}
