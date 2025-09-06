using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/**
**该题目校招岗位可以不作答，社招需要作答。**

按照要求在 {@link Q3.onStartBtnClick} 中编写一段异步任务处理逻辑，具体执行步骤如下：
1. 调用 {@link Q3.loadConfig} 加载配置文件，获取资源列表
2. 根据资源列表调用 {@link Q3.loadFile} 加载资源文件
3. 资源列表中的所有文件加载完毕后，调用 {@link Q3.initSystem} 进行系统初始化
4. 系统初始化完成后，打印日志

附加要求
1. 加载文件时，需要做并发控制，最多并发 3 个文件
2. 加载文件时，需要添加超时控制，超时时间为 3 秒
3. 加载文件失败时，需要对单文件做 backoff retry 处理，重试次数为 3 次
4. 对错误进行捕获并打印输出
*/

public class Q3 : MonoBehaviour
{
    const int maxConcurrent = 3;
    int currentConcurrent = 0;
    object lockObj = new object();
    Queue<TaskCompletionSource<bool>> waitingQueue = new Queue<TaskCompletionSource<bool>>();
    List<Task> loadTasks = new List<Task>();
    public async void OnStartBtnClick()
    {
        // TODO: 请在此处开始作答
        //loadfile我没找到办法从外部停掉，加上超时控制也只能是增加了一个超时重试机制，实际上的loadfile并发并不能完美控制在3个以内，如果完美控制并发的话超时控制就没有意义了。
        
        try
        {
            string[] files = await LoadConfig();

            foreach (var file in files)
            {
                await AcquireSlot();
                loadTasks.Add(LoadFileWithRetryAndTimeout(file, 3, 3000).ContinueWith(t => 
                {
                    ReleaseSlot();
                    return t;
                }).Unwrap());
            }

            
            await Task.WhenAll(loadTasks);
            await InitSystem();
            Debug.Log("Done");
        }
        catch (Exception e)
        {
            Debug.LogError($"OnStartBtnClick failed: {e.Message}");
        }
    }
    Task AcquireSlot()
    {
        lock (lockObj)
        {
            if (currentConcurrent < maxConcurrent)
            {
                currentConcurrent++;
                return Task.CompletedTask;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();
                waitingQueue.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }
    void ReleaseSlot()
    {
        lock (lockObj)
        {
            currentConcurrent--;
            if (waitingQueue.Count > 0)
            {
                var next = waitingQueue.Dequeue();
                currentConcurrent++;
                next.SetResult(true);
            }
        }
    }
    private async Task LoadFileWithRetryAndTimeout(string file, int maxRetryCount, int timeoutMilliseconds)
    {
        int retryCount = 0;
        while (retryCount <= maxRetryCount)
        {
            try
            {
                var loadTask = LoadFile(file);
                var timeoutTask = Task.Delay(timeoutMilliseconds);
                var completedTask = await Task.WhenAny(loadTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException($"Load file {file} timeout");
                }
                else
                {
                    await loadTask;
                    return;
                }
            }
            catch (Exception e)
            {
                retryCount++;
                Debug.Log("load file fail: "+e.Message);
                if (retryCount > maxRetryCount)
                {
                    Debug.LogError($"Failed to load file {file} after {maxRetryCount} retries. Error: {e.Message}");
                    return;
                }
                int delay = (int)Math.Pow(2, retryCount) * 1000;
                ReleaseSlot();
                await Task.Delay(delay);
                await AcquireSlot();
            }
        }
    }

    // #region 以下是辅助测试题而写的一些 mock 函数，请勿修改

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>文件列表</returns>
    public async Task<string[]> LoadConfig()
    {
        Debug.Log("load config start");
        await Task.Delay(1000);
        if (Random.value > 0.01f)
        {
            Debug.Log("load config success");
            string[] files = new string[100];
            for (int i = 0; i < 100; i++)
            {
                files[i] = $"file-{i}";
            }
            return files;
        }
        else
        {
            Debug.Log("load config failed");
            throw new System.Exception("Load config failed");
        }
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public async Task LoadFile(string file)
    {
        Debug.Log($"load file start: {file}");
        await Task.Delay(Random.Range(1000, 5000));
        if (Random.value > 0.01f)
        {
            Debug.Log($"load file success: {file}");
        }
        else
        {
            Debug.Log($"load file failed: {file}");
            throw new System.Exception($"Load file failed: {file}");
        }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    public async Task InitSystem()
    {
        Debug.Log("init system start");
        await Task.Delay(1000);
        Debug.Log("init system success");
    }

    // #endregion
}
