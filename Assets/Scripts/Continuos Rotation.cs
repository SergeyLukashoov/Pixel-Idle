using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ContinuosRotation : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private RotateMode rotateMode = RotateMode.LocalAxisAdd; 
    [SerializeField] private Ease easeType = Ease.Linear;
    [SerializeField] private float absorptionSpeedMultiplier = 3f;
    [SerializeField] private float baseAbsorptionDuration = 1f; // Базовая длительность ускорения за один объект

    private Tweener rotationTweener;
    private float remainingBoostTime = 0f;
    private Coroutine boostCoroutine;

    private void Start()
    {
        StartRotation();
    }

    private void StartRotation()
    {
        rotationTweener = transform.DOLocalRotate(rotationAxis * 360, rotationDuration, rotateMode)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative();
    }

    public void OnAbsorbed()
    {
        // Добавляем время ускорения
        remainingBoostTime += baseAbsorptionDuration;
    
        // Устанавливаем ускоренное вращение
        rotationTweener.timeScale = absorptionSpeedMultiplier;
    
        // Если корутина уже работает, не запускаем новую
        if (boostCoroutine == null)
        {
            boostCoroutine = StartCoroutine(BoostRotationRoutine());
        }
    }

    private IEnumerator BoostRotationRoutine()
    {
        while (remainingBoostTime > 0)
        {
            remainingBoostTime -= Time.deltaTime;
            yield return null;
        }
    
        // Когда время закончилось, возвращаем нормальную скорость
        rotationTweener.timeScale = 1f;
        boostCoroutine = null;
    }

    private void OnDisable()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
        }
    }

    public void StopRotation()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
        }
    }
}