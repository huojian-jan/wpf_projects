namespace Huojian.LibraryManagement.Common.ObjectModel
{
    public class Promise
    {
        private readonly TaskCompletionSource<object> _asyncSource = new TaskCompletionSource<object>();

        public Promise()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; }

        public object Result => _asyncSource.Task.Result;

        public PromiseStates State
        {
            get
            {
                if (_asyncSource.Task.IsCompleted)
                {
                    if (_asyncSource.Task.IsFaulted)
                        return PromiseStates.Rejected;
                    else
                        return PromiseStates.Fulfilled;
                }
                else
                {
                    return PromiseStates.Pending;
                }
            }
        }

        public void Resolve(object result)
        {
            _asyncSource.SetResult(result);
        }

        public void Reject(Exception exception)
        {
            _asyncSource.SetException(exception);
        }
    }


    public enum PromiseStates
    {
        Pending,// 初始状态，既不是成功，也不是失败状态。
        Fulfilled,// 意味着操作成功完成。
        Rejected,// 意味着操作失败。
    }

}
