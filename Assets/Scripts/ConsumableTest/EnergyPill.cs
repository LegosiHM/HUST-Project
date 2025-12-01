using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnergyPill : MonoBehaviour
{
    [SerializeField] private ItemSO _itemSO;
    [SerializeField] private SurvivalStats playerSurvivalStats;

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private float basePointerSpeed = 200f;
    [SerializeField] private WeaponControl weaponControl;

    [SerializeField] private float baseSuccessAreaScale = 0.5f;

    [Header("Audio")]
    [SerializeField] private string useItemSuccessSFX = "sfx_itemused";
    [SerializeField] private string useItemFailSFX = "";

    private float currentSuccessAreaScale;
    private float currentPointerSpeed;

    private float direction = 1f;
    private Vector3 targetPosition;

    private bool isUsingItem;

    void Start()
    {
        currentPointerSpeed = basePointerSpeed;
        currentSuccessAreaScale = baseSuccessAreaScale;

        targetPosition = endPoint.position;

        isUsingItem = false;
    }

    void Update()
    {
        isUsingItem = weaponControl._usingQuacker;


        if (isUsingItem)
        {
            AdjustScaleAndSpeedByBrainwave();
            ShowUI();
            ControlPointerPosition();
        }
        else
        {
            HideUI();
        }
        /*
        if (Keyboard.current.kKey.wasPressedThisFrame) // for test only
        {
            isUsingItem = !isUsingItem;
        }

        if (isUsingItem)
        {
            AdjustScaleAndSpeedByBrainwave();
            ShowUI();
            ControlPointerPosition();
        }
        else
        {
            HideUI();
        }
        */
    }

    private void ControlPointerPosition()
    {
        pointer.position = Vector3.MoveTowards(pointer.position, targetPosition, currentPointerSpeed * Time.deltaTime);

        if(Vector3.Distance(pointer.position, startPoint.position) < 0.1f)
        {
            targetPosition = endPoint.position;
            direction = 1f;
        }
        else if(Vector3.Distance(pointer.position, endPoint.position) < 0.1f)
        {
            targetPosition = startPoint.position;
            direction = -1f;
        }

        /*
        if (Keyboard.current.zKey.wasPressedThisFrame) // for test only
        {
            CheckSuccess();
        }
        */
    }

    private void CheckSuccess()
    {
        if(RectTransformUtility.RectangleContainsScreenPoint(successZone, pointer.position, null))
        {
            playerSurvivalStats.IncreaseBrainwave(-(_itemSO.amountToChangeStat));
            Debug.Log("Success");

            SoundManager.Instance.PlaySFX(useItemSuccessSFX);
        }
        else
        {
            playerSurvivalStats.IncreaseBrainwave(+(_itemSO.amountToChangeStat * 1.5f));
            Debug.Log("Failed");

            SoundManager.Instance.PlaySFX(useItemFailSFX);
        }
    }

    private void ShowUI()
    {
        _canvasGroup.alpha = 1f;
    }

    private void HideUI()
    {
        _canvasGroup.alpha = 0f;

    }

    private void AdjustScaleAndSpeedByBrainwave()
    {
        
        Vector3 successZoneScale = successZone.localScale;

        currentSuccessAreaScale = 1.1f - playerSurvivalStats.currentBrainwave / playerSurvivalStats.maxBrainwave;
        currentSuccessAreaScale = Mathf.Clamp(currentSuccessAreaScale, 0f, 0.6f);

        successZoneScale.x = currentSuccessAreaScale;
        successZone.localScale = successZoneScale;

        currentPointerSpeed = 5f * basePointerSpeed * playerSurvivalStats.currentBrainwave / playerSurvivalStats.maxBrainwave;
        
    }

}
