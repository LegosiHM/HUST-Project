using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilmRollUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image photoImage;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;

    private List<Sprite> photos;
    private int currentIndex = 0;

    // --- Initialization from PhotoCapture ---
    public void Open(List<Sprite> filmRoll)
    {
        photos = filmRoll;
        gameObject.SetActive(true);

        // --- Unlock cursor and pause game ---
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        // Always bind buttons (even if there are no photos)
        leftArrow.onClick.RemoveAllListeners();
        rightArrow.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        leftArrow.onClick.AddListener(() => Move(-1));
        rightArrow.onClick.AddListener(() => Move(1));
        deleteButton.onClick.AddListener(DeleteCurrent);
        closeButton.onClick.AddListener(Close);

        // --- Handle empty film roll ---
        if (photos == null || photos.Count == 0)
        {
            photoImage.sprite = null;
            photoImage.color = new Color(1, 1, 1, 0.2f); // fade to show “no image”
            return;
        }

        photoImage.color = Color.white;
        currentIndex = 0;
        UpdatePhotoDisplay();
    }


    private void Move(int direction)
    {
        if (photos == null || photos.Count == 0) return;

        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = photos.Count - 1;
        else if (currentIndex >= photos.Count)
            currentIndex = 0;

        UpdatePhotoDisplay();
    }

    private void UpdatePhotoDisplay()
    {
        photoImage.sprite = photos[currentIndex];
    }

    private void DeleteCurrent()
    {
        if (photos == null || photos.Count == 0) return;

        photos.RemoveAt(currentIndex);

        if (photos.Count == 0)
        {
            photoImage.sprite = null;
            return;
        }

        if (currentIndex >= photos.Count)
            currentIndex = photos.Count - 1;

        UpdatePhotoDisplay();
    }

    private void Close()
    {
        // --- Lock cursor and resume game ---
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        gameObject.SetActive(false);
    }
}
