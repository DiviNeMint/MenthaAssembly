namespace MenthaAssembly
{
    public delegate TResult OFunc<TParam, out TResult>(out TParam p1);
    public delegate TResult OFunc<TParam1, TParam2, out TResult>(out TParam1 p1, out TParam2 p2);
    public delegate TResult OFunc<TParam1, TParam2, TParam3, out TResult>(out TParam1 p1, out TParam2 p2, out TParam3 p3);

    public delegate TResult RFunc<TParam, out TResult>(ref TParam p1);
    public delegate TResult RFunc<TParam1, TParam2, out TResult>(ref TParam1 p1, ref TParam2 p2);
    public delegate TResult RFunc<TParam1, TParam2, TParam3, out TResult>(ref TParam1 p1, ref TParam2 p2, ref TParam3 p3);

}