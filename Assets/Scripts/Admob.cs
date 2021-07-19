using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class Admob : MonoBehaviour {

	public string appID = "ca-app-pub-3940256099942544~3347511713";
	public string rewardAdID = "ca-app-pub-3940256099942544/5224354917";

	[Space]
	[SerializeField] RevardBox rewardBox;

	RewardBasedVideoAd rewardAd;

	void Start ( ) {
		MobileAds.Initialize ( appID );

		rewardAd = RewardBasedVideoAd.Instance;

		rewardAd.OnAdLoaded += RewardAd_OnAdLoaded;
		rewardAd.OnAdFailedToLoad += RewardAd_OnAdFailedToLoad;
		rewardAd.OnAdClosed += RewardAd_OnAdClosed;
		rewardAd.OnAdRewarded += RewardAd_OnAdRewarded;
	}

	public void RequestRewardAd ( ) 
	{
		rewardAd.LoadAd ( GetNewAdRequest ( ), rewardAdID );
	}

	
	void RewardAd_OnAdRewarded ( object sender, Reward e ) 
	{
		rewardBox.isAdWatched = true;
	}

	void RewardAd_OnAdClosed ( object sender, EventArgs e ) 
	{
		rewardBox.AdClose ( );
	}

	void RewardAd_OnAdFailedToLoad ( object sender, AdFailedToLoadEventArgs e ) 
	{
		rewardBox.AdClose ( );
	}

	void RewardAd_OnAdLoaded ( object sender, EventArgs e ) 
	{
		
		rewardAd.Show ( );
	}

	AdRequest GetNewAdRequest ( )
	{
		return new AdRequest.Builder ( ).Build ( );
	}

	void OnDestroy ( ) 
	{
		rewardAd.OnAdLoaded -= RewardAd_OnAdLoaded;
		rewardAd.OnAdFailedToLoad -= RewardAd_OnAdFailedToLoad;
		rewardAd.OnAdClosed -= RewardAd_OnAdClosed;
		rewardAd.OnAdRewarded -= RewardAd_OnAdRewarded;
	}
}
