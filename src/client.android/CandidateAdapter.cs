﻿using System;
using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Net.Wifi;
using Android.Views;
using Android.Widget;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace client.android
{
    public class CandidateRecord : INotifyPropertyChanged
    {
        public ScanResult ScanResult { get; set; }
        public string Host { get; set; }
        public bool Provisionable { get; set; }
        //public bool Provisioned { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        // lifted from https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string status;

        public string Status
        {
            get { return status;  }
            set { status = value; OnPropertyChanged(); }
        }
    }

    /// <summary>
    /// Interacts with Discover class to reflect progress of discovery
    /// </summary>
    public class CandidateAdapter : BaseAdapter<CandidateRecord>
    {
        readonly Activity activity;

        readonly IList<CandidateRecord> candidateRecords;

        public CandidateAdapter(Activity activity, IList<CandidateRecord> candidates)
        {
            this.activity = activity;

            this.candidateRecords = candidates;
        }

        public override CandidateRecord this[int position]
        {
            get
            {
                return candidateRecords[position];
            }
        }

        public override int Count => candidateRecords.Count;

        public override long GetItemId(int position)
        {
            // TODO: Might want to extract network id, if it's configured
            return position;
        }

        // Guidelines from https://developer.xamarin.com/recipes/android/data/adapters/create_a_custom_adapter_for_contacts/
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? activity.LayoutInflater.Inflate(
                Resource.Layout.CandidateListItem, parent, false);

            var lblSSID = view.FindViewById<TextView>(Resource.Id.lblSSID);
            var lblStatus = view.FindViewById<TextView>(Resource.Id.lblStatus);

            lblSSID.Text = candidateRecords[position].ScanResult.Ssid;
            lblStatus.Text = candidateRecords[position].Status;


            // FIX: clear out propertychanged and reassign it
            // Otherwise we're memory leaking anytime we use convertview
            /*
            if (convertView != null)
            {
                return convertView;
                //candidateRecords[position].PropertyChanged.
            } */

            // TODO: Be sure to work out GC for this kind of operation (hanging an event off
            // a view which may go out of scope)
            candidateRecords[position].PropertyChanged += (o, p) =>
            {
                activity.RunOnUiThread(() =>
                {
                    switch (p.PropertyName)
                    {
                        case nameof(CandidateRecord.Status):
                            lblStatus.Text = candidateRecords[position].Status;
                            break;
                    }
                });
            };

            return view;
        }
    }
}