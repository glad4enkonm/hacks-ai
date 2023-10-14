export interface RenderConfigField {
    type?: string,
    translateKey?: string,
    style?: string,
    disabled?: boolean
  }

export interface RenderConfig {
    [thingName: string]: RenderConfigField
  }