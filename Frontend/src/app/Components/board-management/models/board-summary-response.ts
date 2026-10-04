export interface BoardSummaryResponse {
  id: string;
  boardName: string;
  ownerUserId: string;
  isBoardOwner: boolean;
  columns: BoardColumnSummaryResponse[];
}

export interface BoardColumnSummaryResponse {
  id: string;
  columnName: string;
  sortOrder: number;
  roleIds?: string[];
}
