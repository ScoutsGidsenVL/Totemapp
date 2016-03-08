﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using SQLite;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Totem
{
	[Activity (Label = "Totems")]			
	public class TotemsActivity : Activity
	{
		TotemAdapter totemAdapter;
		ListView totemListView;
		List<Totem> totemList;

		Database db;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Totems);

			db = new Database (this);
	
			int[] totemIDs = Intent.GetIntArrayExtra ("totemIDs");
			int[] freqs = Intent.GetIntArrayExtra ("freqs");

			totemList = ConvertIDArrayToTotemList (totemIDs);

			totemAdapter = new TotemAdapter (this, totemList, freqs);
			totemListView = FindViewById<ListView> (Resource.Id.totem_list);
			totemListView.Adapter = totemAdapter;

			totemListView.ItemClick += listView_ItemClick;

		}

		//fill totemList with Totem-objects whose ID is in totemIDs
		//resulting list is reversed to order them descending by frequency
		private List<Totem> ConvertIDArrayToTotemList(int[] totemIDs) {
			List<Totem> totemList = new List<Totem> ();
			foreach(int idx in totemIDs) {
				totemList.Add (db.GetTotemOnID (idx));
			}

			return totemList;
		}

		void listView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			int pos = e.Position;
			var item = totemAdapter.GetItemAtPosition(pos);

			var detailActivity = new Intent(this, typeof(TotemDetailActivity));
			detailActivity.PutExtra ("totemID", item.nid);
			StartActivity (detailActivity);
		}

		//return to MainActivity and not to EigenschappenActivity when 'back' is pressed
		public override void OnBackPressed() {
			Finish ();
			var intent = new Intent(this, typeof(MainActivity));
			StartActivity (intent);
		}
	}
}

