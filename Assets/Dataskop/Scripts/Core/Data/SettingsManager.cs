using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dataskop
{
    public class SettingsManager : MonoBehaviour {

	    private const string KeyFetchAmount = "fetchAmount";
	    private const string KeyFetchInterval = "fetchInterval";

        public void OnAmountInputChanged(int newValue) {
	        PlayerPrefs.SetInt(KeyFetchAmount, newValue);
        }

        public void OnFetchIntervalInputChanged(int newValue) {
	        PlayerPrefs.SetInt(KeyFetchInterval, newValue * 1000);
        }
    }
}
