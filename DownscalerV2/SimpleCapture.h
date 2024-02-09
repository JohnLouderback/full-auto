#pragma once
#include <winrt/impl/Microsoft.UI.Composition.2.h>

#include "AppState.h"

/**
 * @brief A class representing a simple capture. This class is a simple implementation used to capture the contents of a
 *        window.
 */
class SimpleCapture {
  public:
    /**
     * @brief Initializes a new instance of the SimpleCapture class.
     * @param device The Direct3D 11 device used to interact with the GPU.
     * @param item The capture item used to capture the contents of a window.
     */
    SimpleCapture(
      const winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice& device,
      const winrt::Windows::Graphics::Capture::GraphicsCaptureItem& item
    );
    ~SimpleCapture() { Close(); }

    /**
     * @brief Starts the capture of the window.
     */
    void StartCapture();

    /**
     * @brief Creates a surface given a compositor. The surface is used to display the captured content.
     * @param compositor The compositor used to create the surface.
     * @return The surface created from the capture.
     */
    winrt::Windows::UI::Composition::ICompositionSurface CreateSurface(
      const winrt::Windows::UI::Composition::Compositor& compositor
    );

    /**
     * @brief Closes the capture session. This should be called when the capture is no longer needed.
     */
    void Close();

  private:
    /**
     * @brief Handles the frame arrived event. This is called when a new frame is available to be processed.
     * @param sender The frame pool that the frame arrived from.
     * @param args The arguments for the frame arrived event.
     */
    void OnFrameArrived(
      const winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool& sender,
      const winrt::Windows::Foundation::IInspectable& args
    );

    /**
     * @brief Checks if the capture is closed. If the capture is closed, an exception is thrown.
     * @throws winrt::hresult_error if the capture is closed.
     */
    void CheckClosed() {
      if (closed.load() == true) {
        throw winrt::hresult_error(RO_E_CLOSED);
      }
    }

  private:
    /** The capture item used to capture the contents of a window. */
    winrt::Windows::Graphics::Capture::GraphicsCaptureItem item{nullptr};

    /** The frame pool used to capture frames. */
    winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool framePool{nullptr};

    /** The capture session used to capture frames. */
    winrt::Windows::Graphics::Capture::GraphicsCaptureSession session{nullptr};

    /** The last known size of the capture item. */
    winrt::Windows::Graphics::SizeInt32 lastSize;


    /** The Direct3D 11 device used to interact with the GPU. */
    winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice device{nullptr};

    /** The swap chain used to present the captured content. */
    winrt::com_ptr<IDXGISwapChain1> swapChain{nullptr};

    /** The Direct3D 11 device context used to interact with the GPU. */
    winrt::com_ptr<ID3D11DeviceContext> d3dContext{nullptr};


    /** Indicates if the capture is closed. */
    std::atomic<bool> closed = false;

    /** The revoker used to revoke the frame arrived event. This is used to stop listening for frame arrived events. */
    winrt::Windows::Graphics::Capture::Direct3D11CaptureFramePool::FrameArrived_revoker frameArrived;

    /** A reference to the application state */
    AppState appState;

    /** The last known client rectangle of the window being captured. */
    RECT sourceWindowClientRect;

    /** The last known source box of the window being captured. Used to crop the captured area. */
    D3D11_BOX sourceBox;
};
