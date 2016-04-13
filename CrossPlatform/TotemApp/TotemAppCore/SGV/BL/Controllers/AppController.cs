﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TotemAppCore {
	public class AppController {

		static readonly AppController _instance = new AppController();
		readonly Database database = DatabaseHelper.GetInstance ();

		public delegate void Update();
		public event Update UpdateCounter;

		public delegate void SelectedEigenschappen();
		public event Update ShowSelected;

		List<Totem> _totems;
		List<Eigenschap> _eigenschappen;
		List<Profiel> _profielen;

		NavigationController _navigationController;

		public AppController () {
			_totems = database.GetTotems ();
			_eigenschappen = database.GetEigenschappen ();
			_navigationController = new NavigationController ();
			TotemEigenschapDict = new Dictionary<Totem, int> ();
		}

		public enum DetailMode {
			NORMAL,
			PROFILE,
			RESULT
		}


		/* ------------------------------ PROPERTIES ------------------------------ */


		public static AppController Instance {
			get {
				return _instance;
			}
		}

		public NavigationController NavigationController {
			get {
				return _navigationController;
			}
		}

		public List<Totem> Totems {
			get {
				return _totems;
			}
		}

		public List<Eigenschap> Eigenschappen {
			get {
				return _eigenschappen;
			}
		}

		public List<Profiel> AllProfielen {
			get {
				_profielen = database.GetProfielen ();
				return _profielen;
			}
		}

		public List<Profiel> DistinctProfielen{
			get {
				_profielen = database.GetProfielen (true);
				return _profielen;
			}
		}

		public Totem CurrentTotem { get; set; }
		public DetailMode detailMode { get; set; }

		public Totem NextTotem {
			get {
				List<Totem> list;
				if (detailMode == DetailMode.NORMAL) {
					list = Totems;
				} else if (detailMode == DetailMode.PROFILE) {
					list = GetTotemsFromProfiel (CurrentProfiel.name);
				} else {
					list = TotemEigenschapDict.Keys.ToList ();
				}
				var index = list.FindIndex (x => x.title.Equals (CurrentTotem.title));
				if (index != (list.Count-1))
					return list [index + 1];
				else
					return null;
			}
		}

		public Totem PrevTotem {
			get {
				List<Totem> list;
				if (detailMode == DetailMode.NORMAL) {
					list = Totems;
				} else if (detailMode == DetailMode.PROFILE) {
					list = GetTotemsFromProfiel (CurrentProfiel.name);
				} else {
					list = TotemEigenschapDict.Keys.ToList ();
				}
				var index = list.FindIndex (x => x.title.Equals (CurrentTotem.title));
				if (index != 0)
					return list [index - 1];
				else
					return null;
			}
		}

		public Profiel CurrentProfiel { get; set; }

		public Dictionary<Totem, int> TotemEigenschapDict { get; set; }


		/* ------------------------------ DATABASE ------------------------------ */


		//returns totem-object with given id
		public Totem GetTotemOnID(int idx) {
			foreach(Totem t in _totems)
				if(t.nid.Equals(idx.ToString()))
					return t;

			return null;
		}

		//returns totem-object with given id
		public Totem GetTotemOnID(string idx) {
			return GetTotemOnID (Int32.Parse (idx));
		}

		//returns totem-object with given name
		public List<Totem> FindTotemOpNaam(string name) {
			var result = new List<Totem> ();
			foreach(Totem t in _totems)
				if(t.title.ToLower().Contains(name.ToLower()))
					result.Add (t);

			return result;
		}

		//returns eigenschap-object with given name
		public List<Eigenschap> FindEigenschapOpNaam(string name) {
			var result = new List<Eigenschap> ();
			foreach(Eigenschap e in _eigenschappen)
				if(e.name.ToLower().Contains(name.ToLower()))
					result.Add (e);

			return result;
		}

		//returns a list of totems related to a profile
		public List<Totem> GetTotemsFromProfiel(string name) {
			List<Profiel> list = AllProfielen.FindAll (x=>x.name == name);
			var profiles = list.FindAll (y =>y.nid!=null);
			var result = new List<Totem> ();
			foreach (var profile in profiles) {
				result.Add (GetTotemOnID (profile.nid));
			}
			return result;
		}

		//add totem to profile in db
		public void AddTotemToProfiel(string totemID, string profielName) {
			database.AddTotemToProfiel (totemID,profielName);
		}

		//add a profile
		public void AddProfile(string name){
			database.AddProfile (name);
		}

		//delete a profile
		public void DeleteProfile(string name) {
			database.DeleteProfile (name);
		}

		//delete a totem from a profile
		public void DeleteTotemFromProfile(string totemID, string profielName) {
			database.DeleteTotemFromProfile (totemID,profielName);
		}

		//returns List of Totem_eigenschapp related to eigenschap id
		public List<Totem_eigenschap> GetTotemsVanEigenschapsID(string id) {
			return database.GetTotemsVanEigenschapsID (id);
		}

		//returns Userpref-object based on parameter
		public Userpref GetPreference(string preference) {
			return database.GetPreference (preference);
		}

		//updates the preference with new value
		public void ChangePreference(string preference, string value) {
			database.ChangePreference (preference,value);
		}

		//returns random tip out of the database
		public string GetRandomTip() {
			return database.GetRandomTip ();
		}
		//get list of profile names
		public List<string> GetProfielNamen() {
			var namen = new List<string> ();
			foreach (Profiel p in DistinctProfielen)
				namen.Add (p.name);
			return namen;
		}


		/* ------------------------------ BUTTON CLICK EVENTS ------------------------------ */


		public void TotemMenuItemClicked() {
			_navigationController.GoToTotemList ();
		}

		public void EigenschappenMenuItemClicked() {
			_navigationController.GoToEigenschapList ();
		}

		public void ProfileMenuItemClicked() {
			_navigationController.GoToProfileList ();
		}

		public void ChecklistMenuItemClicked() {
			_navigationController.GoToChecklist ();
		}

		public void TinderMenuItemClicked() {
			_navigationController.GoToTinder ();
		}

		//sets current totem and resets current profile
		public void TotemSelected(string totemID) {
			setCurrentProfile (null);
			setCurrentTotem (totemID);
			_navigationController.GoToTotemDetail ();
		}

		//sets current profile
		public void ProfileSelected(string profileName) {
			setCurrentProfile (profileName);
			_navigationController.GoToProfileTotemList ();
		}

		//sets current totem and current profile
		public void ProfileTotemSelected(string profileName, string totemID) {
			setCurrentProfile (profileName);
			setCurrentTotem (totemID);
			detailMode = DetailMode.PROFILE;
			_navigationController.GoToTotemDetail ();
		}
			
		public void CalculateResultlist(List<Eigenschap> checkboxList) {
			FillAndSortDict (checkboxList);
			detailMode = DetailMode.RESULT;
			_navigationController.GoToTotemResult ();
		}


		/* ------------------------------ MISC ------------------------------ */

			
		void setCurrentTotem(string totemID) {
			var totem = _totems.Find (x => x.nid.Equals (totemID));
			CurrentTotem = totem;
		}

		void setCurrentProfile(string profileName) {
			if (profileName == null) {
				CurrentProfiel = null;
			} else {
				var profiel = _profielen.Find (x => x.name.Equals (profileName));
				CurrentProfiel = profiel;
			}
		}

		//fills dictionary with totems and according frequency based on selected eigenschappen
		void FillAndSortDict(List<Eigenschap> checkboxList) {
			TotemEigenschapDict = new Dictionary<Totem, int> ();
			foreach (Eigenschap eig in checkboxList) {
				if(eig.selected) {
					List<Totem_eigenschap> toAdd = GetTotemsVanEigenschapsID (eig.eigenschapID);
					foreach(Totem_eigenschap totem in toAdd) {
						CollectionHelper.AddOrUpdateDictionaryEntry (TotemEigenschapDict, database.GetTotemsOnId(totem.nid));
					}
				}
			}
			TotemEigenschapDict = CollectionHelper.GetSortedList (TotemEigenschapDict);
		}

		public void FireUpdateEvent() {
			if (UpdateCounter != null)
				UpdateCounter ();
		}

		public void FireSelectedEvent() {
			if (ShowSelected != null)
				ShowSelected ();
		}
	}
}