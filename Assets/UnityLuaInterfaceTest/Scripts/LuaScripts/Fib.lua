-- Copyright (c) 2011 the authors listed at the following URL, and/or
-- the authors of referenced articles or incorporated external code:
-- http://en.literateprograms.org/Fibonacci_numbers_(Lua)?action=history&offset=20081019232035
-- 
-- Permission is hereby granted, free of charge, to any person obtaining
-- a copy of this software and associated documentation files (the
-- "Software"), to deal in the Software without restriction, including
-- without limitation the rights to use, copy, modify, merge, publish,
-- distribute, sublicense, and/or sell copies of the Software, and to
-- permit persons to whom the Software is furnished to do so, subject to
-- the following conditions:
-- 
-- The above copyright notice and this permission notice shall be
-- included in all copies or substantial portions of the Software.
-- 
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
-- EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
-- MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
-- IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
-- CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
-- TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
-- SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
-- 
-- Retrieved from: http://en.literateprograms.org/Fibonacci_numbers_(Lua)?oldid=15146


function fib(n) return n<2 and n or fib(n-1)+fib(n-2) end


function fastfib(n)
	fibs={[0]=0, 1, 1} -- global variable, outside the function

	for i=3,n do
		fibs[i]=fibs[i-1]+fibs[i-2]
	end

	return fibs[n]
end


metafib = { [0]=0, 1, 1 }
local mt = {
	__call = function(t, ...)
		local args = {...}
		local n = args[1]
		
		if not t[n] then
			for i = 3, n do
				t[i] = t[i-2] + t[i-1]
			end
		end

		return t[n]
	end
}

setmetatable(metafib, mt) -- now, metafib can be called as if it were a normal function

-- for n=0,30 do print(fib(n)) end
-- for n=0,30 do print(fastfib(n)) end
-- for n=0,30 do print(metafib(n)) end
