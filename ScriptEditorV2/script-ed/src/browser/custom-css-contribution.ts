import {
  FrontendApplication,
  FrontendApplicationContribution,
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
}
