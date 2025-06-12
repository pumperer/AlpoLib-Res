using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine
{
    public static class AddressableLoaderHelper
    {
        public static async Awaitable Wait(this AsyncOperationHandle operation)
        {
            while (!operation.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }
        }
        
        public static async Awaitable<T> Wait<T>(this AsyncOperationHandle<T> handle)
        {
            while (!handle.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }

            return handle.Result;
        }
        
        public static async Awaitable WhenAll(params AsyncOperationHandle[] handles)
        {
            if (handles.Length == 0)
                return;
            
            while (true)
            {
                var completed = true;
                for (var i = 0; i < handles.Length; i++)
                {
                    var handle = handles[i];
                    completed &= handle.IsDone;
                }

                if (completed)
                    break;
                
                await Awaitable.NextFrameAsync();
            }
        }
        
        public static async Awaitable<T[]> WhenAll<T>(params AsyncOperationHandle<T>[] handles)
        {
            if (handles.Length == 0)
                return null;
            
            var resultArray = new T[handles.Length];
            var completeArray = new bool[handles.Length];
            for (var i = 0; i < handles.Length; i++)
            {
                completeArray[i] = false;
                var handle = handles[i];
                var i1 = i;
                handle.Completed += operationHandle =>
                {
                    completeArray[i1] = true;
                    resultArray[i1] = operationHandle.Result;
                };
            }

            while (true)
            {
                var completed = true;
                for (var i = 0; i < completeArray.Length; i++)
                {
                    if (completeArray[i])
                        continue;
                    
                    completed = false;
                    break;
                }
                if (completed)
                    break;
                await Awaitable.NextFrameAsync();
            }

            return resultArray;
        }
    }
}