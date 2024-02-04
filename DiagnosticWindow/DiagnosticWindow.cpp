#include <windows.h>
#include <sstream>
#include <windowsx.h>

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE, PWSTR pCmdLine, int nCmdShow) {
  // Register the window class.
  constexpr wchar_t CLASS_NAME[] = L"Sample Window Class";

  WNDCLASS wc = {};
  wc.lpfnWndProc = WindowProc;
  wc.hInstance = hInstance;
  wc.lpszClassName = CLASS_NAME;
  RegisterClass(&wc);

  // Create the window.
  auto hwnd = CreateWindowEx(
    0,
    CLASS_NAME,
    L"Mouse Coordinate Display",
    WS_OVERLAPPEDWINDOW,
    CW_USEDEFAULT,
    CW_USEDEFAULT,
    CW_USEDEFAULT,
    CW_USEDEFAULT,
    nullptr,
    nullptr,
    hInstance,
    nullptr
  );

  if (hwnd == nullptr) {
    return 0;
  }

  ShowWindow(hwnd, nCmdShow);

  // Run the message loop.
  MSG msg = {};
  while (GetMessage(&msg, nullptr, 0, 0)) {
    TranslateMessage(&msg);
    DispatchMessage(&msg);
  }

  return 0;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
  static int xPos = -1;
  static int yPos = -1;

  switch (uMsg) {
  case WM_DESTROY:
    PostQuitMessage(0);
    return 0;

  case WM_MOUSEMOVE: {
    xPos = GET_X_LPARAM(lParam);
    yPos = GET_Y_LPARAM(lParam);

    // Trigger a paint message for the window.
    InvalidateRect(hwnd, nullptr, TRUE);
  }
                   return 0;

  case WM_PAINT: {
    PAINTSTRUCT ps;
    auto hdc = BeginPaint(hwnd, &ps);

    // Set up the text to display.
    std::wstringstream ss;
    ss << L"Mouse Position: (" << xPos << L", " << yPos << L")";

    // Draw the text in the client area.
    TextOut(hdc, 5, 5, ss.str().c_str(), ss.str().size());

    EndPaint(hwnd, &ps);
  }
               return 0;
  }

  return DefWindowProc(hwnd, uMsg, wParam, lParam);
}
