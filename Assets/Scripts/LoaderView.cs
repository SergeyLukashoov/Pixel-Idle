using System.Collections;
using DG.Tweening;
using UnityEngine;

public class LoaderView : MonoBehaviour
{
    private Coroutine _loadingCoroutine;

    private bool _canRotate;

    private void OnEnable()
    {
        _loadingCoroutine = StartCoroutine(DelayLoading());
    }

    private void OnDisable()
    {
        StopCoroutine(_loadingCoroutine);
    }

    private IEnumerator DelayLoading()
    {
        int sec = 20;

        while (sec-- > 0)
        {
            Vector3 targetLoadingRotation = new Vector3(0, 0, gameObject.transform.localRotation.eulerAngles.z+350);
            
            gameObject.transform.DORotate(targetLoadingRotation, 1f, RotateMode.WorldAxisAdd);
            
            yield return new WaitForSeconds(1.1f);
        }
    }
}
