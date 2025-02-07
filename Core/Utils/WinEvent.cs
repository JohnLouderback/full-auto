namespace Core.Utils;

/// <summary>
///   Provides constants for accessibility events from "Winuser.h".
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants" />
public static class WinEvent {
    /// <summary>
    ///   The range of WinEvent constant values specified by the Accessibility Interoperability Alliance
    ///   (AIA) for use across the industry. For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_AIA_START = 0xA000;

    /// <summary>
    ///   The range of WinEvent constant values specified by the Accessibility Interoperability Alliance
    ///   (AIA) for use across the industry. For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_AIA_END = 0xAFFF;

    /// <summary>
    ///   The lowest and highest possible event values.
    /// </summary>
    public const uint EVENT_MIN = 0x00000001;

    /// <summary>
    ///   The lowest and highest possible event values.
    /// </summary>
    public const uint EVENT_MAX = 0x7FFFFFFF;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/keyboardshortcut-property">
    ///     KeyboardShortcut
    ///     property
    ///   </see>
    ///   has changed. Server applications send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_ACCELERATORCHANGE = 0x8012;

    /// <summary>
    ///   Sent when a window is cloaked. A cloaked window still exists, but is invisible to the user.
    /// </summary>
    public const uint EVENT_OBJECT_CLOAKED = 0x8017;

    /// <summary>
    ///   A window object's scrolling has ended. Unlike EVENT_SYSTEM_SCROLLINGEND, this event is associated
    ///   with the scrolling window. Whether the scrolling is horizontal or vertical scrolling, this event
    ///   should be sent whenever the scroll action is completed. The hwnd parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describes the scrolling window; the idObject parameter is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_CLIENT</see>
    ///   , and the idChild parameter is CHILDID_SELF.
    /// </summary>
    public const uint EVENT_OBJECT_CONTENTSCROLLED = 0x8015;

    /// <summary>
    ///   An object has been created. The system sends this event for the following user interface
    ///   elements: caret,
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/header-control">header control</see>
    ///   ,
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/list-view-control">
    ///     list-view
    ///     control
    ///   </see>
    ///   ,
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/tab-control">tab control</see>
    ///   ,
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/toolbar-control">
    ///     toolbar
    ///     control
    ///   </see>
    ///   ,
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/tree-view-control">
    ///     tree view
    ///     control
    ///   </see>
    ///   , and
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/window">window</see> object.
    ///   Server applications send this event for their accessible objects. Before sending the event for
    ///   the parent object, servers must send it for all of an object's child objects. Servers must ensure
    ///   that all child objects are fully created and ready to accept
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/oleacc/nn-oleacc-iaccessible">IAccessible</see>
    ///   calls from clients before the parent object sends this event. Because a parent object is created
    ///   after its child objects, clients must make sure that an object's parent has been created before
    ///   calling
    ///   <see
    ///     href="https://learn.microsoft.com/en-us/windows/desktop/api/Oleacc/nf-oleacc-iaccessible-get_accparent">
    ///     IAccessible::get_accParent
    ///   </see>
    ///   , particularly if in-context hook functions are used.
    /// </summary>
    public const uint EVENT_OBJECT_CREATE = 0x8000;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/defaultaction-property">
    ///     DefaultAction
    ///     property
    ///   </see>
    ///   has changed. The system sends this event for dialog boxes. Server applications
    ///   send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_DEFACTIONCHANGE = 0x8011;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/description-property">
    ///     Description
    ///     property
    ///   </see>
    ///   has changed. Server applications send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D;

    /// <summary>
    ///   An object has been destroyed. The system sends this event for the following user interface
    ///   elements: caret, header control, list-view control, tab control, toolbar control, tree view
    ///   control, and window object. Server applications send this event for their accessible objects.
    ///   Clients assume that all of an object's children are destroyed when the parent object sends this
    ///   event. After receiving this event, clients do not call an object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/oleacc/nn-oleacc-iaccessible">IAccessible</see>
    ///   properties or methods. However, the interface pointer must remain valid as long as there is a
    ///   reference count on it (due to COM rules), but the UI element may no longer be present. Further
    ///   calls on the interface pointer may return failure errors; to prevent this, servers
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/creating-proxy-objects">
    ///     create
    ///     proxy objects
    ///   </see>
    ///   and monitor their life spans.
    /// </summary>
    public const uint EVENT_OBJECT_DESTROY = 0x8001;

    /// <summary>
    ///   The user started to drag an element. The hwnd, idObject, and idChild parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the object being dragged.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGSTART = 0x8021;

    /// <summary>
    ///   The user has ended a drag operation before dropping the dragged element on a drop target. The
    ///   hwnd, idObject, and idChild parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the object being dragged.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGCANCEL = 0x8022;

    /// <summary>
    ///   The user dropped an element on a drop target. The hwnd, idObject, and idChild parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the object being dragged.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGCOMPLETE = 0x8023;

    /// <summary>
    ///   The user dragged an element into a drop target's boundary. The hwnd, idObject, and idChild
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the drop target.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGENTER = 0x8024;

    /// <summary>
    ///   The user dragged an element out of a drop target's boundary. The hwnd, idObject, and idChild
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the drop target.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGLEAVE = 0x8025;

    /// <summary>
    ///   The user dropped an element on a drop target. The hwnd, idObject, and idChild parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the drop target.
    /// </summary>
    public const uint EVENT_OBJECT_DRAGDROPPED = 0x8026;

    /// <summary>
    ///   The highest object event value.
    /// </summary>
    public const uint EVENT_OBJECT_END = 0x80FF;

    /// <summary>
    ///   An object has received the keyboard focus. The system sends this event for the following user
    ///   interface elements: list-view control, menu bar, pop-up menu, switch window, tab control, tree
    ///   view control, and window object. Server applications send this event for their accessible
    ///   objects. The hwnd parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identifies the window that receives the keyboard focus.
    /// </summary>
    public const uint EVENT_OBJECT_FOCUS = 0x8005;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/help-property">Help property</see>
    ///   has changed. Server applications send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_HELPCHANGE = 0x8010;

    /// <summary>
    ///   An object is hidden. The system sends this event for the following user interface elements: caret
    ///   and cursor. Server applications send this event for their accessible objects. When this event is
    ///   generated for a parent object, all child objects are already hidden. Server applications do not
    ///   send this event for the child objects. Hidden objects include the
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-state-constants">STATE_SYSTEM_INVISIBLE</see>
    ///   flag; shown objects do not include this flag. The EVENT_OBJECT_HIDE event also indicates that the
    ///   STATE_SYSTEM_INVISIBLE flag is set. Therefore, servers do not send the EVENT_OBJECT_STATECHANGE
    ///   event in this case.
    /// </summary>
    public const uint EVENT_OBJECT_HIDE = 0x8003;

    /// <summary>
    ///   A window that hosts other accessible objects has changed the hosted objects. A client might need
    ///   to query the host window to discover the new hosted objects, especially if the client has been
    ///   monitoring events from the window. A hosted object is an object from an accessibility framework
    ///   (MSAA or UI Automation) that is different from that of the host. Changes in hosted objects that
    ///   are from the same framework as the host should be handed with the structural change events, such
    ///   as EVENT_OBJECT_CREATE for MSAA. For more info see comments within winuser.h.
    /// </summary>
    public const uint EVENT_OBJECT_HOSTEDOBJECTSINVALIDATED = 0x8020;

    /// <summary>
    ///   An IME window has become hidden.
    /// </summary>
    public const uint EVENT_OBJECT_IME_HIDE = 0x8028;

    /// <summary>
    ///   An IME window has become visible.
    /// </summary>
    public const uint EVENT_OBJECT_IME_SHOW = 0x8027;

    /// <summary>
    ///   The size or position of an IME window has changed.
    /// </summary>
    public const uint EVENT_OBJECT_IME_CHANGE = 0x8029;

    /// <summary>
    ///   An object has been invoked; for example, the user has clicked a button. This event is supported
    ///   by common controls and is used by UI Automation. For this event, the hwnd, ID, and idChild
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identify the item that is invoked.
    /// </summary>
    public const uint EVENT_OBJECT_INVOKED = 0x8013;

    /// <summary>
    ///   An object that is part of a live region has changed. A live region is an area of an application
    ///   that changes frequently and/or asynchronously.
    /// </summary>
    public const uint EVENT_OBJECT_LIVEREGIONCHANGED = 0x8019;

    /// <summary>
    ///   An object has changed location, shape, or size. The system sends this event for the following
    ///   user interface elements: caret and window objects. Server applications send this event for their
    ///   accessible objects.  This event is generated in response to a change in the top-level object
    ///   within the object hierarchy; it is not generated for any children that the object might have. For
    ///   example, if the user resizes a window, the system sends this notification for the window, but not
    ///   for the menu bar, title bar, scroll bar, or other objects that have also changed. The system does
    ///   not send this event for every non-floating child window when the parent moves. However, if an
    ///   application explicitly resizes child windows as a result of resizing the parent window, the
    ///   system sends multiple events for the resized children. If an object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/state-property">State property</see>
    ///   is set to
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-state-constants">STATE_SYSTEM_FLOATING</see>
    ///   , the server sends EVENT_OBJECT_LOCATIONCHANGE whenever the object changes location. If an object
    ///   does not have this state, servers only trigger this event when the object moves in relation to
    ///   its parent. For this event notification, the idChild parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identifies the child object that has changed.
    /// </summary>
    public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/name-property">Name property</see>
    ///   has changed. The system sends this event for the following user interface elements: check box,
    ///   cursor, list-view control, push button, radio button, status bar control, tree view control, and
    ///   window object. Server applications send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

    /// <summary>
    ///   An object has a new parent object. Server applications send this event for their accessible
    ///   objects.
    /// </summary>
    public const uint EVENT_OBJECT_PARENTCHANGE = 0x800F;

    /// <summary>
    ///   A container object has added, removed, or reordered its children. The system sends this event for
    ///   the following user interface elements: header control, list-view control, toolbar control, and
    ///   window object. Server applications send this event as appropriate for their accessible objects.
    ///   For example, this event is generated by a list-view object when the number of child elements or
    ///   the order of the elements changes. This event is also sent by a parent window when the Z-order
    ///   for the child windows changes.
    /// </summary>
    public const uint EVENT_OBJECT_REORDER = 0x8004;

    /// <summary>
    ///   The selection within a container object has changed. The system sends this event for the
    ///   following user interface elements: list-view control, tab control, tree view control, and window
    ///   object. Server applications send this event for their accessible objects. This event signals a
    ///   single selection: either a child is selected in a container that previously did not contain any
    ///   selected children, or the selection has changed from one child to another. The hwnd and idObject
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describe the container; the idChild parameter identifies the object that is
    ///   selected. If the selected child is a window that also contains objects, the idChild parameter is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_WINDOW</see>
    ///   .
    /// </summary>
    public const uint EVENT_OBJECT_SELECTION = 0x8006;

    /// <summary>
    ///   A child within a container object has been added to an existing selection. The system sends this
    ///   event for the following user interface elements: list box, list-view control, and tree view
    ///   control. Server applications send this event for their accessible objects. The hwnd and idObject
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describe the container. The idChild parameter is the child that is added to the
    ///   selection.
    /// </summary>
    public const uint EVENT_OBJECT_SELECTIONADD = 0x8007;

    /// <summary>
    ///   An item within a container object has been removed from the selection. The system sends this
    ///   event for the following user interface elements: list box, list-view control, and tree view
    ///   control. Server applications send this event for their accessible objects. This event signals
    ///   that a child is removed from an existing selection.  The hwnd and idObject parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describe the container; the idChild parameter identifies the child that has
    ///   been removed from the selection.
    /// </summary>
    public const uint EVENT_OBJECT_SELECTIONREMOVE = 0x8008;

    /// <summary>
    ///   Numerous selection changes have occurred within a container object. The system sends this event
    ///   for list boxes; server applications send it for their accessible objects. This event is sent when
    ///   the selected items within a control have changed substantially. The event informs the client that
    ///   many selection changes have occurred, and it is sent instead of several EVENT_OBJECT_SELECTIONADD
    ///   or EVENT_OBJECT_SELECTIONREMOVE events. The client queries for the selected items by calling the
    ///   container object's
    ///   <see
    ///     href="https://learn.microsoft.com/en-us/windows/desktop/api/Oleacc/nf-oleacc-iaccessible-get_accselection">
    ///     IAccessible::get_accSelection
    ///   </see>
    ///   method and enumerating the selected items. For this event notification, the hwnd and idObject
    ///   parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describe the container in which the changes occurred.
    /// </summary>
    public const uint EVENT_OBJECT_SELECTIONWITHIN = 0x8009;

    /// <summary>
    ///   A hidden object is shown. The system sends this event for the following user interface elements:
    ///   caret, cursor, and window object. Server applications send this event for their accessible
    ///   objects. Clients assume that when this event is sent by a parent object, all child objects are
    ///   already displayed. Therefore, server applications do not send this event for the child objects.
    ///   Hidden objects include the
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-state-constants">STATE_SYSTEM_INVISIBLE</see>
    ///   flag; shown objects do not include this flag. The EVENT_OBJECT_SHOW event also indicates that the
    ///   STATE_SYSTEM_INVISIBLE flag is cleared. Therefore, servers do not send the
    ///   EVENT_OBJECT_STATECHANGE event in this case.
    /// </summary>
    public const uint EVENT_OBJECT_SHOW = 0x8002;

    /// <summary>
    ///   An object's state has changed. The system sends this event for the following user interface
    ///   elements: check box, combo box, header control, push button, radio button, scroll bar, toolbar
    ///   control, tree view control, up-down control, and window object. Server applications send this
    ///   event for their accessible objects. For example, a state change occurs when a button object is
    ///   clicked or released, or when an object is enabled or disabled. For this event notification, the
    ///   idChild parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identifies the child object whose state has changed.
    /// </summary>
    public const uint EVENT_OBJECT_STATECHANGE = 0x800A;

    /// <summary>
    ///   The conversion target within an IME composition has changed. The conversion target is the subset
    ///   of the IME composition which is actively selected as the target for user-initiated conversions.
    /// </summary>
    public const uint EVENT_OBJECT_TEXTEDIT_CONVERSIONTARGETCHANGED = 0x8030;

    /// <summary>
    ///   An object's text selection has changed. This event is supported by common controls and is used by
    ///   UI Automation. The hwnd, ID, and idChild parameters of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function describe the item that is contained in the updated text selection.
    /// </summary>
    public const uint EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014;

    /// <summary>
    ///   Sent when a window is uncloaked. A cloaked window still exists, but is invisible to the user.
    /// </summary>
    public const uint EVENT_OBJECT_UNCLOAKED = 0x8018;

    /// <summary>
    ///   An object's
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/value-property">Value property</see>
    ///   has changed. The system sends this event for the user interface elements that include the scroll
    ///   bar and the following controls: edit, header, hot key, progress bar, slider, and up-down. Server
    ///   applications send this event for their accessible objects.
    /// </summary>
    public const uint EVENT_OBJECT_VALUECHANGE = 0x800E;

    /// <summary>
    ///   The range of event constant values reserved for OEMs. For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_OEM_DEFINED_START = 0x0101;

    /// <summary>
    ///   The range of event constant values reserved for OEMs. For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_OEM_DEFINED_END = 0x01FF;

    /// <summary>
    ///   An alert has been generated. Server applications should not send this event.
    /// </summary>
    public const uint EVENT_SYSTEM_ALERT = 0x0002;

    /// <summary>
    ///   A preview rectangle is being displayed.
    /// </summary>
    public const uint EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016;

    /// <summary>
    ///   A window has lost mouse capture. This event is sent by the system, never by servers.
    /// </summary>
    public const uint EVENT_SYSTEM_CAPTUREEND = 0x0009;

    /// <summary>
    ///   A window has received mouse capture. This event is sent by the system, never by servers.
    /// </summary>
    public const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;

    /// <summary>
    ///   A window has exited context-sensitive Help mode. This event is not sent consistently by the
    ///   system.
    /// </summary>
    public const uint EVENT_SYSTEM_CONTEXTHELPEND = 0x000D;

    /// <summary>
    ///   A window has entered context-sensitive Help mode. This event is not sent consistently by the
    ///   system.
    /// </summary>
    public const uint EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C;

    /// <summary>
    ///   The active desktop has been switched.
    /// </summary>
    public const uint EVENT_SYSTEM_DESKTOPSWITCH = 0x0020;

    /// <summary>
    ///   A dialog box has been closed. The system sends this event for standard dialog boxes; servers send
    ///   it for custom dialog boxes. This event is not sent consistently by the system.
    /// </summary>
    public const uint EVENT_SYSTEM_DIALOGEND = 0x0011;

    /// <summary>
    ///   A dialog box has been displayed. The system sends this event for standard dialog boxes, which are
    ///   created using resource templates or Win32 dialog box functions. Servers send this event for
    ///   custom dialog boxes, which are windows that function as dialog boxes but are not created in the
    ///   standard way.  This event is not sent consistently by the system.
    /// </summary>
    public const uint EVENT_SYSTEM_DIALOGSTART = 0x0010;

    /// <summary>
    ///   An application is about to exit drag-and-drop mode. Applications that support drag-and-drop
    ///   operations must send this event; the system does not send this event.
    /// </summary>
    public const uint EVENT_SYSTEM_DRAGDROPEND = 0x000F;

    /// <summary>
    ///   An application is about to enter drag-and-drop mode. Applications that support drag-and-drop
    ///   operations must send this event because the system does not send it.
    /// </summary>
    public const uint EVENT_SYSTEM_DRAGDROPSTART = 0x000E;

    /// <summary>
    ///   The highest system event value.
    /// </summary>
    public const uint EVENT_SYSTEM_END = 0x00FF;

    /// <summary>
    ///   The foreground window has changed. The system sends this event even if the foreground window has
    ///   changed to another window in the same thread. Server applications never send this event.  For
    ///   this event, the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function's hwnd parameter is the handle to the window that is in the foreground, the
    ///   idObject parameter is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_WINDOW</see>
    ///   , and the idChild parameter is CHILDID_SELF.
    /// </summary>
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    /// <summary>
    ///   A pop-up menu has been closed. The system sends this event for standard menus; servers send it
    ///   for custom menus. When a pop-up menu is closed, the client receives this message, and then the
    ///   EVENT_SYSTEM_MENUEND event. This event is not sent consistently by the system.
    /// </summary>
    public const uint EVENT_SYSTEM_MENUPOPUPEND = 0x0007;

    /// <summary>
    ///   A pop-up menu has been displayed. The system sends this event for standard menus, which are
    ///   identified by HMENU, and are created using menu-template resources or Win32 menu functions.
    ///   Servers send this event for custom menus, which are user interface elements that function as
    ///   menus but are not created in the standard way. This event is not sent consistently by the system.
    /// </summary>
    public const uint EVENT_SYSTEM_MENUPOPUPSTART = 0x0006;

    /// <summary>
    ///   A menu from the menu bar has been closed. The system sends this event for standard menus; servers
    ///   send it for custom menus.  For this event, the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function's hwnd, idObject, and idChild parameters refer to the control that contains the
    ///   menu bar or the control that activates the context menu. The hwnd parameter is the handle to the
    ///   window that is related to the event. The idObject parameter is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_MENU</see>
    ///   or
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_SYSMENU</see>
    ///   for a menu, or
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_WINDOW</see>
    ///   for a pop-up menu. The idChild parameter is CHILDID_SELF.
    /// </summary>
    public const uint EVENT_SYSTEM_MENUEND = 0x0005;

    /// <summary>
    ///   A menu item on the menu bar has been selected. The system sends this event for standard menus,
    ///   which are identified by HMENU, created using menu-template resources or Win32 menu API elements.
    ///   Servers send this event for custom menus, which are user interface elements that function as
    ///   menus but are not created in the standard way. For this event, the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function's hwnd, idObject, and idChild parameters refer to the control that contains the
    ///   menu bar or the control that activates the context menu. The hwnd parameter is the handle to the
    ///   window related to the event. The idObject parameter is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_MENU</see>
    ///   or
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_SYSMENU</see>
    ///   for a menu, or
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_WINDOW</see>
    ///   for a pop-up menu. The idChild parameter is CHILDID_SELF.  The system triggers more than one
    ///   EVENT_SYSTEM_MENUSTART event that does not always correspond with the EVENT_SYSTEM_MENUEND event.
    /// </summary>
    public const uint EVENT_SYSTEM_MENUSTART = 0x0004;

    /// <summary>
    ///   A window object is about to be restored. This event is sent by the system, never by servers.
    /// </summary>
    public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

    /// <summary>
    ///   A window object is about to be minimized. This event is sent by the system, never by servers.
    /// </summary>
    public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;

    /// <summary>
    ///   The movement or resizing of a window has finished. This event is sent by the system, never by
    ///   servers.
    /// </summary>
    public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

    /// <summary>
    ///   A window is being moved or resized. This event is sent by the system, never by servers.
    /// </summary>
    public const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;

    /// <summary>
    ///   Scrolling has ended on a scroll bar. This event is sent by the system for standard scroll bar
    ///   controls and for scroll bars that are attached to a window. Servers send this event for custom
    ///   scroll bars, which are user interface elements that function as scroll bars but are not created
    ///   in the standard way. The idObject parameter that is sent to the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_HSCROLL</see>
    ///   for horizontal scroll bars, and
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_VSCROLL</see>
    ///   for vertical scroll bars.
    /// </summary>
    public const uint EVENT_SYSTEM_SCROLLINGEND = 0x0013;

    /// <summary>
    ///   Scrolling has started on a scroll bar. The system sends this event for standard scroll bar
    ///   controls and for scroll bars attached to a window. Servers send this event for custom scroll
    ///   bars, which are user interface elements that function as scroll bars but are not created in the
    ///   standard way. The idObject parameter that is sent to the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function is
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_HSCROLL</see>
    ///   for horizontal scrolls bars, and
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_VSCROLL</see>
    ///   for vertical scroll bars.
    /// </summary>
    public const uint EVENT_SYSTEM_SCROLLINGSTART = 0x0012;

    /// <summary>
    ///   A sound has been played. The system sends this event when a system sound, such as one for a menu,
    ///   is played even if no sound is audible (for example, due to the lack of a sound file or a sound
    ///   card). Servers send this event whenever a custom UI element generates a sound.  For this event,
    ///   the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function receives the
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/object-identifiers">OBJID_SOUND</see>
    ///   value as the idObject parameter.
    /// </summary>
    public const uint EVENT_SYSTEM_SOUND = 0x0001;

    /// <summary>
    ///   The user has released ALT+TAB. This event is sent by the system, never by servers. The hwnd
    ///   parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identifies the window to which the user has switched. If only one application
    ///   is running when the user presses ALT+TAB, the system sends this event without a corresponding
    ///   EVENT_SYSTEM_SWITCHSTART event.
    /// </summary>
    public const uint EVENT_SYSTEM_SWITCHEND = 0x0015;

    /// <summary>
    ///   The user has pressed ALT+TAB, which activates the switch window. This event is sent by the
    ///   system, never by servers. The hwnd parameter of the
    ///   <see href="https://learn.microsoft.com/en-us/windows/desktop/api/Winuser/nc-winuser-wineventproc">WinEventProc</see>
    ///   callback function identifies the window to which the user is switching. If only one application
    ///   is running when the user presses ALT+TAB, the system sends an EVENT_SYSTEM_SWITCHEND event
    ///   without a corresponding EVENT_SYSTEM_SWITCHSTART event.
    /// </summary>
    public const uint EVENT_SYSTEM_SWITCHSTART = 0x0014;

    /// <summary>
    ///   The range of event constant values reserved for UI Automation event identifiers. For more
    ///   information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_UIA_EVENTID_START = 0x4E00;

    /// <summary>
    ///   The range of event constant values reserved for UI Automation event identifiers. For more
    ///   information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_UIA_EVENTID_END = 0x4EFF;

    /// <summary>
    ///   The range of event constant values reserved for UI Automation property-changed event identifiers.
    ///   For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_UIA_PROPID_START = 0x7500;

    /// <summary>
    ///   The range of event constant values reserved for UI Automation property-changed event identifiers.
    ///   For more information, see
    ///   <see href="https://learn.microsoft.com/en-us/windows/win32/winauto/allocation-of-winevent-ids">
    ///     Allocation
    ///     of WinEvent IDs
    ///   </see>
    ///   .
    /// </summary>
    public const uint EVENT_UIA_PROPID_END = 0x75FF;
}
