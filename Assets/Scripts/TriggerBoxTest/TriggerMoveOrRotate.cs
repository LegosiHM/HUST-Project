using System.Collections;
using UnityEngine;

public class TriggerMoveOrRotate : MonoBehaviour
{
    [SerializeField] private Transform _linkedObject;

    [Header("Move")]
    [SerializeField] private Vector3 _movedBy;
    [SerializeField] private float _moveSpeed = 10f;

    [Header("Rotate")]
    [SerializeField] private Vector3 _rotateBy;
    [SerializeField] private float _rotateSpeed = 10f;

    [Header("Other")]
    [SerializeField] private string _sound;
    [SerializeField] private bool _isOneTime = true;

    private Coroutine _rotateRoutine;
    private Coroutine _moveRoutine;

    private bool _moveFinished = false;
    private bool _rotateFinished = false;



    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerContext>() != null)
        {
            //put play sound script here

            Rotate();
            Move();
        }

    }


    private void Rotate()
    {
        if (_rotateRoutine != null)
            StopCoroutine(_rotateRoutine);

        _rotateRoutine = StartCoroutine(RotateRoutine());
    }
    private IEnumerator RotateRoutine()
    {
        Quaternion startRot = _linkedObject.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(_rotateBy);

        float t = 0f;
        float duration = 1f;
        while (t < 1f)
        {
            float speedMultiplier = Mathf.Lerp(1f, 2f, t);

            t += Time.deltaTime * _rotateSpeed * speedMultiplier;

            float lerpT = Mathf.Clamp01(t);

            _linkedObject.rotation = Quaternion.Slerp(startRot, targetRot, lerpT);

            yield return null;
        }

        _linkedObject.rotation = targetRot;
        _rotateFinished = true;
        CheckIfBothDone();
    }


    private void Move()
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(MoveRoutine());

    }
    private IEnumerator MoveRoutine()
    {
        Vector3 startPos = _linkedObject.position;
        Vector3 targetPos = startPos + _movedBy;

        while (Vector3.Distance(_linkedObject.position, targetPos) > 0.01f)
        {
            _linkedObject.position = Vector3.MoveTowards(
                _linkedObject.position,
                targetPos,
                _moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        _linkedObject.position = targetPos;
        _moveFinished = true;
        CheckIfBothDone();
    }

    private void CheckIfBothDone()
    {
        if (_moveFinished && _rotateFinished && _isOneTime)
        {
            Destroy(gameObject);
        }
    }


}
