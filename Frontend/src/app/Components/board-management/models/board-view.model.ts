import { WorkflowColumn } from "./workflow-column-request";

export interface BoardView {
  id: string;
  name: string;
  isBoardOwner: boolean;
  workflowColumns: WorkflowColumn[];
}

export interface BoardNavigationState {
  board?: import("./board-summary-response").BoardSummaryResponse;
}
