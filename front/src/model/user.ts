export interface IUser {
  userId?: bigint,
  token?: string,
  login: string,
  passwordHash?: string
  telegram?: string
  name?: string,
  description?: string,
}

export interface IUserStable {
  userId: bigint,
  token?: string,
  login: string,
  passwordHash?: string,
  telegram?: string,
  name: string,
  description?: string
}