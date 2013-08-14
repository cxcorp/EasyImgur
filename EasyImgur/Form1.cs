﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyImgur
{
    public partial class Form1 : Form
    {
        private bool CloseCommandWasSentFromExitButton = false;

        public Form1()
        {
            InitializeComponent();

            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.VisibleChanged += new System.EventHandler(this.Form1_VisibleChanged);
            notifyIcon1.BalloonTipClicked += new System.EventHandler(this.NotifyIcon1_BalloonTipClicked);
            notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon1_MouseDoubleClick);

            ImgurAPI.obtainedAuthorization += new ImgurAPI.ObtainedAuthorization(this.ObtainedAPIAuthorization);
            ImgurAPI.lostAuthorization += new ImgurAPI.LostAuthorization(this.LostAPIAuthorization);

            notifyIcon1.ShowBalloonTip(2000, "EasyImgur", "Right-click EasyImgur's icon here to use it!", ToolTipIcon.Info);
        }

        private void ObtainedAPIAuthorization()
        {
            checkBoxUseAccount.Enabled = true;
            label4.Visible = false;
            label13.Text = "Authorized";
            label13.ForeColor = System.Drawing.Color.Green;
        }

        private void LostAPIAuthorization()
        {
            checkBoxUseAccount.Enabled = false;
            label4.Visible = true;
            label13.Text = "Not authorized";
            label13.ForeColor = System.Drawing.Color.DarkBlue;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void trayMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCommandWasSentFromExitButton = true;
            Application.Exit();
        }

        private void uploadClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            APIResponses.ImageResponse resp = null;
            Image clipboardImage = null;
            string clipboardURL = string.Empty;
            if (Clipboard.ContainsImage())
            {
                clipboardImage = Clipboard.GetImage();
                notifyIcon1.ShowBalloonTip(4000, "Hold on...", "Attempting to upload image to Imgur...", ToolTipIcon.None);
                resp = ImgurAPI.UploadImage(clipboardImage, GetTitleString(), GetDescriptionString());
            }
            else if (Clipboard.ContainsText())
            {
                clipboardURL = Clipboard.GetText(TextDataFormat.UnicodeText);
                Uri uriResult;
                if (Uri.TryCreate(clipboardURL, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    notifyIcon1.ShowBalloonTip(4000, "Hold on...", "Attempting to upload image to Imgur...", ToolTipIcon.None);
                    resp = ImgurAPI.UploadImage(clipboardURL, GetTitleString(), GetDescriptionString());
                }
                else
                {
                    notifyIcon1.ShowBalloonTip(2000, "Can't upload clipboard!", "There's text on the clipboard but it's not a valid URL", ToolTipIcon.Error);
                    return;
                }
            }
            else
            {
                notifyIcon1.ShowBalloonTip(2000, "Can't upload clipboard!", "There's no image or URL there", ToolTipIcon.Error);
                return;
            }

            if (Properties.Settings.Default.copyLinks)
            {
                Clipboard.SetText(resp.data.link);
            }
            if (resp.success)
            {
                notifyIcon1.ShowBalloonTip(2000, "Success!", Properties.Settings.Default.copyLinks ? "Link copied to clipboard" : "Upload placed in history: " + resp.data.link, ToolTipIcon.None);

                HistoryItem item = new HistoryItem();
                item.id = resp.data.id;
                item.link = resp.data.link;
                item.deletehash = resp.data.deletehash;
                item.title = resp.data.title;
                item.description = resp.data.description;
                if (clipboardImage != null)
                {
                    item.thumbnail = clipboardImage.GetThumbnailImage(pictureBox1.Width, pictureBox1.Height, null, System.IntPtr.Zero);
                }
                listBoxHistory.Items.Add(item);
            }
            else
            {
                notifyIcon1.ShowBalloonTip(2000, "Failed", "Could not upload image (" + resp.status + "):", ToolTipIcon.None);
            }

            if (!Properties.Settings.Default.clearClipboardOnUpload)
            {
                if (clipboardImage != null)
                {
                    Clipboard.SetImage(clipboardImage);
                }
                else
                {
                    Clipboard.SetText(clipboardURL, TextDataFormat.UnicodeText);
                }
            }
        }

        private void uploadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            DialogResult res = dialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                using (System.IO.Stream stream = dialog.OpenFile())
                {
                    Image img = System.Drawing.Image.FromStream(stream);
                    notifyIcon1.ShowBalloonTip(2000, "Hold on...", "Attempting to upload image to Imgur...", ToolTipIcon.None);
                    APIResponses.ImageResponse resp = ImgurAPI.UploadImage(img, GetTitleString(), GetDescriptionString());
                    if (Properties.Settings.Default.copyLinks)
                    {
                        Clipboard.SetText(resp.data.link);
                    }
                    if (resp.success)
                    {
                        notifyIcon1.ShowBalloonTip(2000, "Success!", Properties.Settings.Default.copyLinks ? "Link copied to clipboard" : "Upload placed in history: " + resp.data.link, ToolTipIcon.None);

                        HistoryItem item = new HistoryItem();
                        item.id = resp.data.id;
                        item.link = resp.data.link;
                        item.deletehash = resp.data.deletehash;
                        item.title = resp.data.title;
                        item.description = resp.data.description;
                        item.thumbnail = img.GetThumbnailImage(pictureBox1.Width, pictureBox1.Height, null, System.IntPtr.Zero);
                        listBoxHistory.Items.Add(item);
                    }
                    else
                    {
                        notifyIcon1.ShowBalloonTip(2000, "Failed", "Could not upload image (" + resp.status + "):", ToolTipIcon.None);
                    }
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open settings form.
            this.Show();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
            //this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.Activate();
            this.Focus();
            this.BringToFront();
        }

        private void NotifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            /*if (!Properties.Settings.Default.copyLinks)
            {

            }*/
            this.Show();
            tabControl1.SelectedIndex = 2;
            listBoxHistory.SelectedIndex = listBoxHistory.Items.Count - 1;
            this.BringToFront();
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            // Assign control values.
            checkBoxClearClipboard.Checked = Properties.Settings.Default.clearClipboardOnUpload;
            checkBoxCopyLinks.Checked = Properties.Settings.Default.copyLinks;
            textBoxTitleFormat.Text = Properties.Settings.Default.titleFormat;
            textBoxDescriptionFormat.Text = Properties.Settings.Default.descriptionFormat;
            comboBoxImageFormat.SelectedIndex = Properties.Settings.Default.imageFormat;
            checkBoxUseAccount.Checked = Properties.Settings.Default.useAccount;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Form1_VisibleChanged(null, null);
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            this.Hide();
            e.Cancel = !CloseCommandWasSentFromExitButton;  // Don't want to *actually* close the form unless the Exit button was used.
        }

        private void SaveSettings()
        {
            // Store control values.
            Properties.Settings.Default.clearClipboardOnUpload = checkBoxClearClipboard.Checked;
            Properties.Settings.Default.copyLinks = checkBoxCopyLinks.Checked;
            Properties.Settings.Default.titleFormat = textBoxTitleFormat.Text;
            Properties.Settings.Default.descriptionFormat = textBoxDescriptionFormat.Text;
            Properties.Settings.Default.imageFormat = comboBoxImageFormat.SelectedIndex;
            Properties.Settings.Default.useAccount = checkBoxUseAccount.Checked;

            Properties.Settings.Default.Save();
        }

        private void buttonChangeCredentials_Click(object sender, EventArgs e)
        {
            AuthorizeForm accountCredentialsForm = new AuthorizeForm();
            DialogResult res = accountCredentialsForm.ShowDialog(this);
            
            if (ImgurAPI.HasBeenAuthorized())
            {
                buttonForceTokenRefresh.Enabled = true;
            }
            else
            {
                buttonForceTokenRefresh.Enabled = false;
            }
        }

        private void listBoxHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            HistoryItem item = listBoxHistory.SelectedItem as HistoryItem;
            if (item != null)
            {
                textBoxID.Text = item.id;
                textBoxLink.Text = item.link;
                textBoxDeleteHash.Text = item.deletehash;
                pictureBox1.Image = item.thumbnail;

                buttonRemoveFromImgur.Enabled = true;
            }
            else
            {
                textBoxID.Text = string.Empty;
                textBoxLink.Text = string.Empty;
                textBoxDeleteHash.Text = string.Empty;
                pictureBox1.Image = null;

                buttonRemoveFromImgur.Enabled = false;
            }
        }

        private string FormatInfoString( string _Input )
        {
            string formatted = _Input;
            formatted = formatted.Replace("%n%", ImgurAPI.numSuccessfulUploads.ToString());
            formatted = formatted.Replace("%date%", "%day%-%month%-%year%");
            formatted = formatted.Replace("%time%", "%hour%:%minute%:%second%");
            formatted = formatted.Replace("%day%", System.DateTime.Now.Day.ToString());
            formatted = formatted.Replace("%month%", System.DateTime.Now.Month.ToString());
            formatted = formatted.Replace("%year%", System.DateTime.Now.Year.ToString());
            formatted = formatted.Replace("%hour%", System.DateTime.Now.Hour.ToString());
            formatted = formatted.Replace("%minute%", System.DateTime.Now.Minute.ToString());
            formatted = formatted.Replace("%second%", System.DateTime.Now.Second.ToString());
            return formatted;
        }

        private string GetTitleString()
        {
            return FormatInfoString(textBoxTitleFormat.Text);
        }

        private string GetDescriptionString()
        {
            return FormatInfoString(textBoxDescriptionFormat.Text);
        }

        private void buttonRemoveFromImgur_Click(object sender, EventArgs e)
        {
            HistoryItem item = listBoxHistory.SelectedItem as HistoryItem;
            if (item == null)
            {
                return;
            }

            notifyIcon1.ShowBalloonTip(2000, "Hold on...", "Attempting to remove image from Imgur...", ToolTipIcon.None);
            if (ImgurAPI.DeleteImage(item.deletehash))
            {
                listBoxHistory.Items.Remove(item);
                notifyIcon1.ShowBalloonTip(2000, "Success!", "Removed image from Imgur and history", ToolTipIcon.None);
            }
            else
            {
                notifyIcon1.ShowBalloonTip(2000, "Failed", "Failed to remove image from Imgur", ToolTipIcon.Error);
            }

            listBoxHistory.SelectedItem = null;
            listBoxHistory_SelectedIndexChanged(null, null);
        }

        private void buttonForceTokenRefresh_Click(object sender, EventArgs e)
        {
            ImgurAPI.ForceRefreshTokens();
        }
    }
}