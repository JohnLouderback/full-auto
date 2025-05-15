import {
  FrontendApplication,
  FrontendApplicationContribution
} from "@theia/core/lib/browser";
import { injectable } from "@theia/core/shared/inversify";

@injectable()
export class CustomCssContribution implements FrontendApplicationContribution {
  onStart(app: FrontendApplication): void {
    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.href = "../style/styles.css";
    document.head.appendChild(link);
  }

  async initializeLayout(app: FrontendApplication): Promise<void> {
    const layoutData = app.shell.getLayoutData();
    console.log("Getting layout data");
    console.log(layoutData);
    try {
      // Show the left side bar Explorer Panel.
      layoutData.activeWidgetId = "explorer-view-container";

      // Expand the Outline View.
      layoutData.rightPanel!.items!.find(
        (item) => item.widget!.id === "outline-view"
      )!.expanded = true;
    } catch (e) {
      console.error(e);
    }
    await app.shell.setLayoutData(layoutData);
  }
}
