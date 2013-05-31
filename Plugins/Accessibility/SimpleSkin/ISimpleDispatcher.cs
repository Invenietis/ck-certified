using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SimpleSkin
{
    public interface ISimpleDispatcher
    {
        DispatcherOperation BeginInvoke( Delegate method, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate asynchronously at the specified priority
        //     on the thread the System.Windows.Threading.Dispatcher is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     The delegate to a method that takes no arguments, which is pushed onto the
        //     System.Windows.Threading.Dispatcher event queue.
        //
        // Returns:
        //     An object, which is returned immediately after Overload:System.Windows.Threading.Dispatcher.BeginInvoke
        //     is called, that can be used to interact with the delegate as it is pending
        //     execution in the event queue.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     method is null.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid System.Windows.Threading.DispatcherPriority.
        DispatcherOperation BeginInvoke( DispatcherPriority priority, Delegate method );
        //
        // Summary:
        //     Executes the specified delegate asynchronously with the specified arguments,
        //     at the specified priority, on the thread that the System.Windows.Threading.Dispatcher
        //     was created on.
        //
        // Parameters:
        //   method:
        //     The delegate to a method that takes parameters specified in args, which is
        //     pushed onto the System.Windows.Threading.Dispatcher event queue.
        //
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   args:
        //     An array of objects to pass as arguments to the given method. Can be null.
        //
        // Returns:
        //     An object, which is returned immediately after Overload:System.Windows.Threading.Dispatcher.BeginInvoke
        //     is called, that can be used to interact with the delegate as it is pending
        //     execution in the event queue.
        DispatcherOperation BeginInvoke( Delegate method, DispatcherPriority priority, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate asynchronously at the specified priority
        //     and with the specified argument on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     A delegate to a method that takes one argument, which is pushed onto the
        //     System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     The object to pass as an argument to the specified method.
        //
        // Returns:
        //     An object, which is returned immediately after Overload:System.Windows.Threading.Dispatcher.BeginInvoke
        //     is called, that can be used to interact with the delegate as it is pending
        //     execution in the event queue.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     method is null.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid System.Windows.Threading.DispatcherPriority.
        DispatcherOperation BeginInvoke( DispatcherPriority priority, Delegate method, object arg );
        //
        // Summary:
        //     Executes the specified delegate asynchronously at the specified priority
        //     and with the specified array of arguments on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     A delegate to a method that takes multiple arguments, which is pushed onto
        //     the System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     The object to pass as an argument to the specified method.
        //
        //   args:
        //     An array of objects to pass as arguments to the specified method.
        //
        // Returns:
        //     An object, which is returned immediately after Overload:System.Windows.Threading.Dispatcher.BeginInvoke
        //     is called, that can be used to interact with the delegate as it is pending
        //     execution in the System.Windows.Threading.Dispatcher queue.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     method is null.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     System.Windows.Threading.DispatcherPriority is not a valid priority.
        DispatcherOperation BeginInvoke( DispatcherPriority priority, Delegate method, object arg, params object[] args );

        object Invoke( Delegate method, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate synchronously at the specified priority on
        //     the thread on which the System.Windows.Threading.Dispatcher is associated
        //     with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     A delegate to a method that takes no arguments, which is pushed onto the
        //     System.Windows.Threading.Dispatcher event queue.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     priority is equal to System.Windows.Threading.DispatcherPriority.Inactive.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid priority.
        //
        //   System.ArgumentNullException:
        //     method is null.
        object Invoke( DispatcherPriority priority, Delegate method );
        //
        // Summary:
        //     Executes the specified delegate at the specified priority with the specified
        //     arguments synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   method:
        //     A delegate to a method that takes parameters specified in args, which is
        //     pushed onto the System.Windows.Threading.Dispatcher event queue.
        //
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   args:
        //     An array of objects to pass as arguments to the given method. Can be null.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        object Invoke( Delegate method, DispatcherPriority priority, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate within the designated time span at the specified
        //     priority with the specified arguments synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   method:
        //     A delegate to a method that takes parameters specified in args, which is
        //     pushed onto the System.Windows.Threading.Dispatcher event queue.
        //
        //   timeout:
        //     The maximum amount of time to wait for the operation to complete.
        //
        //   args:
        //     An array of objects to pass as arguments to the given method. Can be null.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        object Invoke( Delegate method, TimeSpan timeout, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate at the specified priority with the specified
        //     argument synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     A delegate to a method that takes one argument, which is pushed onto the
        //     System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     An object to pass as an argument to the given method.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     priority is equal to System.Windows.Threading.DispatcherPriority.Inactive.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid priority.
        //
        //   System.ArgumentNullException:
        //     method is null.
        object Invoke( DispatcherPriority priority, Delegate method, object arg );
        //
        // Summary:
        //     Executes the specified delegate synchronously at the specified priority and
        //     with the specified time-out value on the thread the System.Windows.Threading.Dispatcher
        //     was created.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   timeout:
        //     The maximum time to wait for the operation to finish.
        //
        //   method:
        //     The delegate to a method that takes no arguments, which is pushed onto the
        //     System.Windows.Threading.Dispatcher event queue.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        object Invoke( DispatcherPriority priority, TimeSpan timeout, Delegate method );
        //
        // Summary:
        //     Executes the specified delegate within the designated time span at the specified
        //     priority with the specified arguments synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   method:
        //     A delegate to a method that takes parameters specified in args, which is
        //     pushed onto the System.Windows.Threading.Dispatcher event queue.
        //
        //   timeout:
        //     The maximum amount of time to wait for the operation to complete.
        //
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   args:
        //     An array of objects to pass as arguments to the given method. Can be null.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        object Invoke( Delegate method, TimeSpan timeout, DispatcherPriority priority, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate at the specified priority with the specified
        //     arguments synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   method:
        //     A delegate to a method that takes multiple arguments, which is pushed onto
        //     the System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     An object to pass as an argument to the given method.
        //
        //   args:
        //     An array of objects to pass as arguments to the given method.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     priority is equal to System.Windows.Threading.DispatcherPriority.Inactive.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid priority.
        //
        //   System.ArgumentNullException:
        //     method is null.
        object Invoke( DispatcherPriority priority, Delegate method, object arg, params object[] args );
        //
        // Summary:
        //     Executes the specified delegate at the specified priority with the specified
        //     argument synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   timeout:
        //     The maximum time to wait for the operation to finish.
        //
        //   method:
        //     A delegate to a method that takes multiple arguments, which is pushed onto
        //     the System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     An object to pass as an argument to the given method. This can be null if
        //     no arguments are needed.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     priority is equal to System.Windows.Threading.DispatcherPriority.Inactive.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid priority.
        //
        //   System.ArgumentNullException:
        //     method is null.
        object Invoke( DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg );
        //
        // Summary:
        //     Executes the specified delegate at the specified priority with the specified
        //     arguments synchronously on the thread the System.Windows.Threading.Dispatcher
        //     is associated with.
        //
        // Parameters:
        //   priority:
        //     The priority, relative to the other pending operations in the System.Windows.Threading.Dispatcher
        //     event queue, the specified method is invoked.
        //
        //   timeout:
        //     The maximum time to wait for the operation to finish.
        //
        //   method:
        //     A delegate to a method that takes multiple arguments, which is pushed onto
        //     the System.Windows.Threading.Dispatcher event queue.
        //
        //   arg:
        //     An object to pass as an argument to the specified method.
        //
        //   args:
        //     An array of objects to pass as arguments to the specified method.
        //
        // Returns:
        //     The return value from the delegate being invoked or null if the delegate
        //     has no return value.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     priority is equal to System.Windows.Threading.DispatcherPriority.Inactive.
        //
        //   System.ComponentModel.InvalidEnumArgumentException:
        //     priority is not a valid System.Windows.Threading.DispatcherPriority.
        //
        //   System.ArgumentNullException:
        //     method is null.
        object Invoke( DispatcherPriority priority, TimeSpan timeout, Delegate method, object arg, params object[] args );
    }
}
