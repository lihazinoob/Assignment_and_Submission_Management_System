import type { JwtClaims } from "@/types/auth"

export function decodeJwt(token: string): JwtClaims {
  const payload = token.split(".")[1]
  const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"))
  const claims = JSON.parse(decoded)

  return {
    sub: claims["sub"] ?? claims["nameid"] ?? claims[Object.keys(claims).find((k) => k.endsWith("nameidentifier")) ?? "sub"],
    email: claims["email"] ?? claims[Object.keys(claims).find((k) => k.endsWith("emailaddress")) ?? "email"],
    role: claims["role"] ?? claims[Object.keys(claims).find((k) => k.endsWith("/role")) ?? "role"],
    name: claims["name"] ?? claims[Object.keys(claims).find((k) => k.endsWith("/name")) ?? "name"],
    exp: claims["exp"],
  }
}
