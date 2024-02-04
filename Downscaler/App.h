#pragma once

class SimpleCapture;

/**
 * @brief The App class is responsible for initializing the application and starting the capture.
 */
class App {
  public:
    App() = default;
    ~App() = default;

    /**
     * @brief Initializes the application given the root visual.
     * @param root The root visual of the application. The root visual is the parent of all other visuals in the
     *             application.
     */
    void Initialize(
      const winrt::Windows::UI::Composition::ContainerVisual& root
    );

    /**
     * @brief Starts the capture of the window that was set in the AppState.
     */
    void StartCapture();

    /**
     * @brief Starts the capture of a given window for its handle.
     * @param hwnd The window to capture.
     */
    void StartCapture(HWND hwnd);

  private:
    /** The compositor used to create visuals. */
    winrt::Windows::UI::Composition::Compositor compositor{nullptr};

    /** The root visual of the application. */
    winrt::Windows::UI::Composition::ContainerVisual root{nullptr};

    /** The content visual of the application. This is where the captured content will be displayed. */
    winrt::Windows::UI::Composition::SpriteVisual content{nullptr};

    /** The brush used to display the captured content. This is used to alter the appearance of the content. */
    winrt::Windows::UI::Composition::CompositionSurfaceBrush brush{nullptr};

    /** The Direct3D 11 device used to interact with the GPU. */
    winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice device{nullptr};

    /** The capture object used as the mechanism to capture the content of a window. */
    std::unique_ptr<SimpleCapture> capture{nullptr};
};
