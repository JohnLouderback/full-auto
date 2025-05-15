export type File = string;

export interface Directory {
  [key: string]: File | Directory;
}
