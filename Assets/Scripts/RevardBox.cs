using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RevardBox : MonoBehaviour
{
	public enum UserRewardType
	{
		Coins
	}

	[Serializable]
	public struct UserReward
	{
		public UserRewardType RewardType;
		public Sprite Icon;
		public int Amount;
	}

	[SerializeField] GameObject rewardBoxUICanvas;
	[SerializeField] Transform rewardsParent;
	[SerializeField] Transform rewardsCheckMarksParent;
	[SerializeField] GameObject noMoreRewardsPanel;

	[Space]
	[Header("Progress Bar UI")]
	[SerializeField] Image progressBarFill;

	[Space]
	[Header("Remaining Ads UI & Watch Ad Button")]
	[SerializeField] GameObject remainingAdsBadge;
	Text remainingAdsBadgeText;

	[Space]
	[SerializeField] Button watchAdButton;
	Text watchAdButtonText;
	string watchAdButtonDefaultText;

	[Space]
	[SerializeField] Text watchedAdsText;

	[Space]
	[Header("Coins & Gems Text UI")]
	[SerializeField] Text coinsText;

	[Space]
	[Header("Rewards FX")]
	[SerializeField] ParticleSystem coinsRewardFx;
	
	[Space]
	[Header("Admob reference")]
	[SerializeField] Admob admob;

	[Space]
	[Header("Time to wait (Minutes) before activating Rewards again")]
	public double waitTimeToActivateRewards;

	[Space]
	[Header("Rewards Informations")]
	const int TOTAL_REWARDS = 6;
	public UserReward[] userRewards = new UserReward[TOTAL_REWARDS];

	static UserReward currentReward;
	static int currentRewardIndex = 0;

	public bool isAdWatched;

	void Awake()
	{
		remainingAdsBadgeText = remainingAdsBadge.transform.GetChild(0).GetComponent<Text>();
		watchAdButtonText = watchAdButton.transform.GetChild(0).GetComponent<Text>();
		watchAdButtonDefaultText = watchAdButtonText.text;
	}

	void Start()
	{
		CheckForAvailableRewards();

		DrawRewardsUI();

		UpdateCoinsTextUI();		

		UpdateRemainingRewardsTextUI();
		UpdateWatchedADsTextUI();
	}

	void DrawRewardsUI()
	{
		for (int i = currentRewardIndex; i < TOTAL_REWARDS; i++)
		{
			UserReward reward = userRewards[i];

			rewardsParent.GetChild(i).GetChild(1).GetComponent<Image>().sprite = reward.Icon;			
			rewardsParent.GetChild(i).GetChild(2).GetComponent<Text>().text = reward.Amount.ToString();
		}
	}

	public void WatchAdButtonClick()
	{
		isAdWatched = false;
		watchAdButton.interactable = false;
		watchAdButtonText.text = "LOADING...";
#if UNITY_EDITOR
		StartCoroutine(SimulateEditorRequestRewardAd());
#elif UNITY_ANDROID
		admob.RequestRewardAd();

#endif
	}

#if UNITY_EDITOR
	IEnumerator SimulateEditorRequestRewardAd()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 1.3f));

		isAdWatched = true;
		AdClose();
	}
#endif

	public void AdClose()
	{
		watchAdButtonText.text = watchAdButtonDefaultText;

		if (isAdWatched)
		{
			
			watchAdButton.interactable = false;
			currentReward = userRewards[currentRewardIndex];
			currentRewardIndex++;
			float progressValue = (float)currentRewardIndex / TOTAL_REWARDS;

			progressBarFill.DOFillAmount(progressValue, 1.5f).OnComplete(RewardUser);

		}
		else
		{			
			watchAdButton.interactable = true;
		}
	}

	void RewardUser()
	{
		watchAdButton.interactable = true;

		
		if (currentReward.RewardType == UserRewardType.Coins)
		{
			
			Debug.Log("<color=orange>Coins Reward : +" + currentReward.Amount + "</color>");

			GameData.Coins += currentReward.Amount;
			UpdateCoinsTextUI();

			coinsRewardFx.Play();


		}

		UpdateRemainingRewardsTextUI();
		UpdateWatchedADsTextUI();

		MarkRewardAsCheked(currentRewardIndex - 1);

		
		PlayerPrefs.SetInt("CurrentRewardIndex", currentRewardIndex);

		if (currentRewardIndex == TOTAL_REWARDS)
		{
			PlayerPrefs.SetString("RewardsCompletionDateTime", DateTime.Now.ToString());
		}
	}

	void MarkRewardAsCheked(int rewardIndex)
	{
		
		rewardsParent.GetChild(rewardIndex).gameObject.SetActive(false);
		rewardsCheckMarksParent.GetChild(rewardIndex).gameObject.SetActive(true);

		float progressValue = (float)currentRewardIndex / TOTAL_REWARDS;
		progressBarFill.fillAmount = progressValue;
		
		if (rewardIndex == TOTAL_REWARDS - 1)
		{
			watchAdButton.interactable = false;
			remainingAdsBadge.SetActive(false);
			noMoreRewardsPanel.SetActive(true);

			currentRewardIndex = TOTAL_REWARDS;
		}
	}

	void CheckForAvailableRewards()
	{
		currentRewardIndex = PlayerPrefs.GetInt("CurrentRewardIndex", 0);

		
		if (currentRewardIndex == TOTAL_REWARDS)
		{
			DateTime rewardsCompletionDateTime = DateTime.Parse(PlayerPrefs.GetString("RewardsCompletionDateTime", DateTime.Now.ToString()));
			DateTime currentDateTime = DateTime.Now;

			double elapsedMinutes = (currentDateTime - rewardsCompletionDateTime).TotalMinutes;

			Debug.Log("Time Passed Since Last Reward: " + elapsedMinutes);

			if (elapsedMinutes >= waitTimeToActivateRewards)
			{
				
				PlayerPrefs.SetString("RewardsCompletionDateTime", "");
				PlayerPrefs.SetInt("CurrentRewardIndex", 0);
				currentRewardIndex = 0;

			}
			else
			{
				Debug.Log("wait for " + (waitTimeToActivateRewards - elapsedMinutes) + " Minutes");
			}
		}

		if (currentRewardIndex > 0)
		{
			for (int i = 0; i < currentRewardIndex; i++)
			{
				MarkRewardAsCheked(i);
			}
		}
	}

	void UpdateRemainingRewardsTextUI()
	{
		remainingAdsBadgeText.text = (TOTAL_REWARDS - currentRewardIndex).ToString();
	}

	void UpdateWatchedADsTextUI()
	{
		watchedAdsText.text = string.Format("{0}/{1}", currentRewardIndex, TOTAL_REWARDS);
	}

	void UpdateCoinsTextUI()
	{
		coinsText.text = GameData.Coins.ToString();
	}

	
	public void OpenUI()
	{
		rewardBoxUICanvas.SetActive(true);
	}

	public void CloseUI()
	{
		rewardBoxUICanvas.SetActive(false);
	}

	
#if UNITY_EDITOR
	void Update()
	{
		if (Input.GetKeyUp(KeyCode.Delete))
		{
			PlayerPrefs.DeleteAll();
			Debug.Log("Player Prefs deleted ...");
		}
	}
#endif
}
