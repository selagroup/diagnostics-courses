#include <stdlib.h>
#include <stdio.h>
typedef int __WORD;
static __WORD __SCRATCH1;
static __WORD __SCRATCH2;
#define __ALLOC(n) malloc(n)
#define __FREE(p) free(p)
#define STACK_SIZE 1000
static int __SP = 0;
static __WORD __STACK[STACK_SIZE];
#define __PUSH(w) __STACK[__SP++] = w 
#define __POP() (__STACK[--__SP]) 
static __WORD Array__new() {
  return (__WORD)__ALLOC(__POP()*sizeof(__WORD));
}
__WORD Memory__free() {
  __FREE((void*)__POP());
  return 0;
}
static __WORD System__print() {
  printf("%s", (char*)__POP());
  return 0;
}
static __WORD System__println() {
  printf("\n");
  return 0;
}
static __WORD System__printInt() {
  printf("%d", (int)__POP());
  return 0;
}
static __WORD System__readInt() {
  int i;
  scanf_s("%d", &i);
  return i;
}
int main() {
  Main__main();
  return 0;
}
